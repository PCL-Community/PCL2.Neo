using PCL.Neo.Core.Utils;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace PCL.Neo.Core.Download;

public class Downloader(int degreeOfParallelism = 8)
{
    public bool IsCancelled => _cancellationTokenSource.IsCancellationRequested;

    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

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

    public async Task Download<T>(T receipts) where T : IEnumerable<DownloadReceipt>
    {
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
                        while (_transferRateRecords.TryDequeue(out var x) &&
                               x.Item1 < DateTime.Now - TimeSpan.FromSeconds(1.5))
                        {
                        }
                    }
                }
            }, daemonCts.Token);
        }
        catch (OperationCanceledException) { }

        var immutableReceipts = receipts.ToImmutableArray();
        _downloadReceipts.AddRange(immutableReceipts);


        var receiptExecutor = new ActionBlock<DownloadReceipt>(
            async r => await r.DownloadAsync(_client, false, _cancellationTokenSource.Token),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = degreeOfParallelism,
                CancellationToken = _cancellationTokenSource.Token
            });

        foreach (DownloadReceipt r in immutableReceipts)
        {
            var origProgress = r.DeltaSizeProgress;
            r.DeltaSizeProgress = new SynchronousProgress<long>(x =>
            {
                _transferRateRecordChannel.Writer.TryWrite((DateTime.Now, x));
                origProgress?.Report(x);
            });
            receiptExecutor.Post(r);
        }

        receiptExecutor.Complete();
        try
        {
            await receiptExecutor.Completion.WaitAsync(CancellationToken.None);
        }
        catch (OperationCanceledException) { }

        await daemonCts.CancelAsync();
    }

    public void Cancel() => _cancellationTokenSource.Cancel();
    public async Task CancelAsync() => await _cancellationTokenSource.CancelAsync();
}