using PCL.Neo.Core.Utils;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace PCL.Neo.Core.Download;

public class Downloader(int degreeOfParallelism = 8)
{
    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _maxThreadsThrottle = new(degreeOfParallelism, degreeOfParallelism);

    private readonly List<DownloadReceipt> _downloadReceipts = [];
    private readonly List<Task> _downloadTasks = [];
    private readonly ConcurrentQueue<(DateTime, long)> _transferRateRecords = [];
    private readonly int _maxTransferRateRecordSize = degreeOfParallelism * 256;
    private readonly Channel<(DateTime, long)> _transferRateRecordChannel = Channel.CreateUnbounded<(DateTime, long)>();

    public double Progress
    {
        get
        {
            var a = _downloadReceipts.Sum(x => x.Size);
            var b = _downloadReceipts.Sum(x => x.TotalSize != 0 ? x.TotalSize : x.Integrity?.ExpectedSize ?? 0);
            return b == 0 ? 0 : (double)a / b;
        }
    }

    public long TransferRate => _transferRateRecords
        .Where(x => DateTime.Now - x.Item1 <= TimeSpan.FromSeconds(1))
        .Sum(x => x.Item2);

    private void CheckTransferRate(long numberOfBytes)
    {
        // _transferRateRecords.Enqueue((DateTime.Now, numberOfBytes));
        // while (_transferRateRecords.Count > MaxTransferRateRecordSize)
        // {
        //     _transferRateRecords.TryDequeue(out _);
        // }
        _transferRateRecordChannel.Writer.WriteAsync((DateTime.Now, numberOfBytes))
            .AsTask().GetAwaiter().GetResult();
    }

    public async Task Download<T>(T receipts) where T : IEnumerable<DownloadReceipt>
    {
        var immutableReceipts = receipts.ToImmutableArray();
        _downloadReceipts.AddRange(immutableReceipts);
        foreach (DownloadReceipt r in immutableReceipts)
        {
            var origProgress = r.DeltaSizeProgress;
            r.DeltaSizeProgress = new SynchronousProgress<long>(x =>
            {
                CheckTransferRate(x);
                origProgress?.Report(x);
            });
            _downloadTasks.Add(r.DownloadInNewTask(_client, _maxThreadsThrottle, _cancellationTokenSource.Token));
        }

        var daemonCts = new CancellationTokenSource();
        try
        {
            _ = Task.Run(async () =>
            {
                (DateTime, long) cached = (DateTime.Now, 0);
                await foreach (var r in _transferRateRecordChannel.Reader.ReadAllAsync(daemonCts.Token))
                {
                    if ((r.Item1 - cached.Item1).Duration() < TimeSpan.FromMilliseconds(10))
                    {
                        cached.Item2 += r.Item2;
                    }
                    else
                    {
                        _transferRateRecords.Enqueue(cached);
                        cached = (DateTime.Now, 0);
                    }
                    if (_transferRateRecords.Count >= _maxTransferRateRecordSize)
                    {
                        while (_transferRateRecords.TryDequeue(out var x) && x.Item1 < DateTime.Now - TimeSpan.FromSeconds(1.5))
                        {
                        }
                    }
                }
            }, daemonCts.Token);
        }
        catch (OperationCanceledException) { }

        await Task.WhenAll(_downloadTasks);
        await daemonCts.CancelAsync();
    }

    public void Cancel() => _cancellationTokenSource.Cancel();
}