using System.Collections.Immutable;

namespace PCL.Neo.Core.Download;

public class Downloader(int degreeOfParallelism = 8)
{
    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _maxThreadsThrottle = new(degreeOfParallelism, degreeOfParallelism);

    private readonly List<DownloadReceipt> _downloadReceipts = [];

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

    public long TransferRate => _downloadReceipts
        .Where(x => !x.IsCompleted)
        .Sum(x => x.TransferRate);

    public async Task Download<T>(T receipts) where T : IEnumerable<DownloadReceipt>
    {
        var immutableReceipts = receipts.ToImmutableArray();
        _downloadReceipts.AddRange(immutableReceipts);
        foreach (DownloadReceipt r in immutableReceipts)
        {
            r.DownloadInNewTask(_client, _maxThreadsThrottle, _cancellationTokenSource.Token);
        }

        await Task.WhenAll(immutableReceipts.Select(x => x.DownloadTask)!);
    }

    public void Cancel() => _cancellationTokenSource.Cancel();
}