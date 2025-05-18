using System.Security.Cryptography;

namespace PCL.Neo.Core.Download;

file class SynchronousProgress<T>(Action<T> action) : IProgress<T>
{
    public void Report(T value)
    {
        action(value);
    }
}

file static class StreamExt
{
    // https://gist.github.com/dalexsoto/9fd3c5bdbe9f61a717d47c5843384d11
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize,
        IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        if (source is null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}

public class FileIntegrityException(string? msg = null) : Exception(msg);

public class DownloadReceipt
{
    protected static HttpClient SharedClient = new();

    public event Action<DownloadReceipt>? OnBegin;
    public event Action<DownloadReceipt>? OnSuccess;
    public event Action<DownloadReceipt, Exception>? OnError;

    public string SourceUrl { get; init; } = string.Empty;
    public string DestinationPath { get; init; } = string.Empty;
    public FileIntegrity? Integrity { get; init; }
    public int MaxRetries { get; init; } = 3;
    public int Attempts { get; private set; }
    public long Size { get; private set; }
    public long TotalSize { get; private set; }

    public bool IsCompleted { get; private set; }

    public long TransferRate { get; private set; } // in byte per second

    public IProgress<double>? Progress { get; init; }
    public Task? DownloadTask { get; private set; }

    // For internal use
    private DateTime _sizeChangedTime = DateTime.MinValue;
    private long _lastSize;

    public Task DownloadInNewTask(HttpClient? client = null, SemaphoreSlim? maxThreadsThrottle = null,
        CancellationToken token = default)
    {
        try
        {
            return DownloadTask = Task.Run(async () => await DownloadAsync(client, maxThreadsThrottle, token), token);
        }
        catch (TaskCanceledException) { }

        return Task.FromCanceled(token);
    }

    public async Task DownloadAsync(HttpClient? client = null, SemaphoreSlim? maxThreadsThrottle = null,
        CancellationToken token = default)
    {
        IsCompleted = false;
        client ??= SharedClient;
        try
        {
            await using (var fs = new FileStream(
                             DestinationPath + ".tmp", // to ensure only properly downloaded file exists
                             FileMode.Create,
                             FileAccess.ReadWrite, FileShare.None))
                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    await (maxThreadsThrottle?.WaitAsync(token) ?? Task.CompletedTask);
                    try
                    {
                        OnBegin?.Invoke(this);

                        var res = await client.GetAsync(SourceUrl, HttpCompletionOption.ResponseHeadersRead, token);
                        res.EnsureSuccessStatusCode();

                        fs.SetLength(0); // clear file content
                        Size = 0;
                        _lastSize = 0;
                        _sizeChangedTime = DateTime.Now;
                        TotalSize = res.Content.Headers.ContentLength ?? 0;
                        TransferRate = 0;

                        // preparing parent directory
                        var parentDir = Path.GetDirectoryName(DestinationPath);
                        if (!string.IsNullOrEmpty(parentDir))
                            Directory.CreateDirectory(parentDir);

                        // copying file content
                        await using (var ns = await res.Content.ReadAsStreamAsync(token))
                        {
                            await ns.CopyToAsync(
                                fs,
                                81920,
                                new SynchronousProgress<long>(x =>
                                {
                                    Size = x;
                                    double deltaS = Size - _lastSize;
                                    _lastSize = Size;
                                    double deltaT = (DateTime.Now - _sizeChangedTime).TotalSeconds;
                                    _sizeChangedTime = DateTime.Now;
                                    TransferRate = (long)(deltaS / deltaT);
                                    Progress?.Report((double)Size / TotalSize);
                                }),
                                token);
                        }

                        TransferRate = 0;

                        if (!Integrity?.Verify(fs) ?? false)
                            throw new FileIntegrityException($"Failed to verify integrity for {SourceUrl}");

                        IsCompleted = true;
                        OnSuccess?.Invoke(this);
                        break; // downloaded successfully, break download loop
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException && Attempts < MaxRetries)
                    {
                        // 異議あり！ ...
                        const int baseDelayMs = 500;
                        int delay = baseDelayMs * (1 << Attempts++);
                        // TODO: remove this testing log
                        Console.WriteLine(
                            $"[{SourceUrl}] Attempt {Attempts} failed: {ex.Message}. Retry after {delay} ms...");
                        await Task.Delay(delay, token);
                    }
                    finally
                    {
                        maxThreadsThrottle?.Release();
                    }
                }

            if (IsCompleted)
                File.Move(DestinationPath + ".tmp", DestinationPath);
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            // 寄
            OnError?.Invoke(this, ex);
        }
        finally
        {
            // clean up
            if (File.Exists(DestinationPath + ".tmp"))
                File.Delete(DestinationPath + ".tmp");
        }
    }

    public static async Task FastDownload(string sourceUrl, string destPath, string? sha1 = null) =>
        await new DownloadReceipt
        {
            SourceUrl = sourceUrl,
            DestinationPath = destPath,
            Integrity = sha1 is null ? null : new FileIntegrity { HashAlgorithm = SHA1.Create(), Hash = sha1 }
        }.DownloadAsync();

    public static async Task<FileStream> FastDownloadAsStream(string sourceUrl, string destPath, string? sha1 = null)
    {
        await FastDownload(sourceUrl, destPath, sha1);
        return new FileStream(destPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}