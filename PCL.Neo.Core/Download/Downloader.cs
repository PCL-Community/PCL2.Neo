using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace PCL.Neo.Core.Download;

public class Downloader(int degreeOfParallelism = 8)
{
    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _maxThreadsThrottle = new(degreeOfParallelism, degreeOfParallelism);

    private readonly List<DownloadReceipt> _downloadReceipts = [];
    private readonly ConcurrentQueue<(DateTime, long)> _transferRateRecords = [];
    private const int MaxTransferRateRecordSize = 16384;

    public double Progress
    {
        get
        {
            var receipts = _downloadReceipts
                .Where(x => x.Integrity is { ExpectedSize: >= 0 })
                .ToImmutableArray();
            return (double)receipts
                .Sum(x => x.Size) / receipts.Sum(x => x.Integrity!.ExpectedSize); // TODO: divided by zero
        }
    }

    public long TransferRate => _transferRateRecords
        .Where(x => DateTime.Now - x.Item1 <= TimeSpan.FromSeconds(1))
        .Sum(x => x.Item2);

    private void CheckTransferRate(long numberOfBytes)
    {
        _transferRateRecords.Enqueue((DateTime.Now, numberOfBytes));
        while (_transferRateRecords.Count > MaxTransferRateRecordSize)
        {
            _transferRateRecords.TryDequeue(out _);
        }
    }

    public async Task Download<T>(T receipts) where T : IEnumerable<DownloadReceipt>
    {
        var immutableReceipts = receipts.ToImmutableArray();
        _downloadReceipts.AddRange(immutableReceipts);
        foreach (DownloadReceipt r in immutableReceipts)
        {
            var origProgress = r.Progress;
            r.Progress = new Progress<double>(x =>
            {
                CheckTransferRate(r.DeltaSize);
                origProgress?.Report(x);
            });
            r.DownloadInNewTask(_client, _maxThreadsThrottle, _cancellationTokenSource.Token);
        }

        await Task.WhenAll(immutableReceipts.Select(x => x.DownloadTask)!);
    }

    public void Cancel() => _cancellationTokenSource.Cancel();
}