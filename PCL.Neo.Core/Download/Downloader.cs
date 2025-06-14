using PCL.Neo.Core.Utils;
using System.Net.Http.Headers;

namespace PCL.Neo.Core.Download
{
    public class Downloader : IDisposable
    {
        private DownloadTask[] _tasks;
        private int            _concurrency;

        private long _total = -1;
        private long _downloaded = 0;

        public struct DownloadProgress(long total, long downloaded, double percentage)
        {
            public long Total = total;
            public long Downloaded = downloaded;
            public double Percentage = percentage;
        }

        public event Action<DownloadProgress>? OnDownloadProgressChanged;

        private DownloadProgress Progress
        {
            get
            {
                long total = _total;
                long downloaded = _downloaded;
                double percentage = 1.0 * downloaded / total;
                return new DownloadProgress(total, downloaded, percentage);
            }
        }

        /// <summary>
        /// 初始化 Downloader
        /// </summary>
        public Downloader(DownloadTask[] tasks, int concurrency = 16)
        {
            _tasks = tasks;
            _concurrency = concurrency;

            var  len   = _tasks.Length;
            long total = 0;

            for (var i = 0; i < len; i++)
            {
                if (_tasks[i].End == -1)
                {
                    HttpRequestMessage request = new(HttpMethod.Head, _tasks[i].Url);
                    long contentLength =
                        _client.Send(request, HttpCompletionOption.ResponseHeadersRead).Content.Headers.ContentLength ?? -1;
                    _tasks[i].End = contentLength;
                }

                if (_tasks[i].End != -1)
                    total += _tasks[i].End - _tasks[i].Start + 1;
            }

            _perTask = (long) Math.Ceiling(1.0 * total / _concurrency);
            long present = _tasks[0].Start;
            long delta   = 0;
            foreach (var task in _tasks)
            {
                if (task.End == -1)
                {
                    _unknownTasks.Enqueue(task);
                }

                if (delta > 0)
                {
                    if (delta > task.End - task.Start + 1)
                    {
                        _rangedTasks.Enqueue(new DownloadTask(task.Url, task.Destination, task.Start, task.End));
                        delta -= task.End - task.Start + 1;
                        continue;
                    }
                    present = delta + task.Start;
                    _rangedTasks.Enqueue(new DownloadTask(task.Url, task.Destination, task.Start, present - 1)); 
                }

                while (present + _perTask <= task.End + 1)
                {
                    _rangedTasks.Enqueue(new DownloadTask(task.Url, task.Destination,present, present + _perTask - 1));
                    present += _perTask;
                    delta   =  0;
                }

                if (present + _perTask >= task.End)
                {
                    _rangedTasks.Enqueue(new DownloadTask(task.Url, task.Destination, present, task.End));
                    delta  = _perTask - (task.End - present + 1);
                }
            }

            // create target storage directory
            var destDir = Path.GetDirectoryName(_tasks[0].Destination);
            if (!Directory.Exists(destDir))
            {
                ArgumentException.ThrowIfNullOrEmpty(destDir, nameof(destDir));
                Directory.CreateDirectory(destDir);
            }
        }

        /// <summary>
        /// Represents a download task with a specified URL, destination path, and byte range.
        /// </summary>
        /// <param name="url">The URL to download the content from.</param>
        /// <param name="dest">The destination file path where the content will be saved.</param>
        /// <param name="start">The starting byte position for the download range.</param>
        /// <param name="end">The ending byte position for the download range.</param>
        public struct DownloadTask(string url, string dest, long start = 0, long end = -1)
        {
            public readonly string Url   = url;
            public readonly long   Start = start;
            public          long   End   = end;
            public readonly string Destination  = dest;
        };

        private readonly HttpClient              _client            = new();
        private          Queue<DownloadTask>     _rangedTasks       = new();
        private          Queue<DownloadTask>     _unknownTasks      = new();
        private          Queue<DownloadTask>     _receivedTasks     = new();
        private          long                    _perTask           = 0;
        private readonly CancellationTokenSource _cancellationToken = new();

        private void Cancelled()
        {
            lock (_receivedTasks)
                foreach (var task in _receivedTasks)
                {
                    var filePath = $"{task.Destination}.{task.Start}-{task.End}.tmp";
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            _isFinished.Set();
        }

        public static bool CheckIsEnoughSpace(DownloadTask task)
        {
            try
            {
                var drive = new DriveInfo(task.Destination);
                return drive.IsReady && drive.AvailableFreeSpace > task.End - task.Start + 1;
            }
            catch (ArgumentException _)
            {
                return false;
            }
        }

        /// <summary>
        /// 下载 Range 不为空的任务
        /// </summary>
        private async Task DownloadRanged()
        {
            Queue<DownloadTask> tasks = new();
            lock (_rangedTasks)
            {
                long current = 0;
                while (current < _perTask && _rangedTasks.Count > 0)
                {
                    _cancellationToken.Token.ThrowIfCancellationRequested();
                    var popped = _rangedTasks.Dequeue();
                    tasks.Enqueue(popped);
                    lock (_receivedTasks)
                        _receivedTasks.Enqueue(popped);
                    current += popped.End - popped.Start + 1;
                }
            }
            foreach (var task in tasks)
            {
                // build http request
                var request = new HttpRequestMessage(HttpMethod.Get, task.Url);
                request.Headers.Range = new RangeHeaderValue(task.Start, task.End);

                // send request
                var response = await _client.SendAsync(request, _cancellationToken.Token);

                if (CheckIsEnoughSpace(task))
                {
                    await using var fs = new FileStream($"{task.Destination}.{task.Start}-{task.End}.tmp",
                        FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    // copy stream
                    var stream = await response.Content.ReadAsStreamAsync(_cancellationToken.Token);
                    await stream.CopyToAsync(fs, task.End - task.Start + 1, new SynchronousProgress<long>(l =>
                    {
                        _downloaded = l;
                        OnDownloadProgressChanged?.Invoke(Progress);
                    }), _cancellationToken.Token);
                }
                else
                {
                    throw new IOException("The desk is full.");
                }
            }
        }

        /// <summary>
        /// 下载 Range 为空的任务
        /// </summary>
        private async Task<bool> DownloadUnknown()
        {
            DownloadTask task;
            lock (_unknownTasks)
            {
                if (_unknownTasks.Count == 0)
                    return false;
                task = _unknownTasks.Dequeue();
            }
            lock (_receivedTasks)
                _receivedTasks.Enqueue(task);

            var             res = await _client.GetStreamAsync(task.Url, _cancellationToken.Token);
            await using var fs  = new FileStream(task.Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (CheckIsEnoughSpace(task))
            {
                await res.CopyToAsync(fs, _cancellationToken.Token);
            }
            else
            {
                throw new IOException("The desk is full.");
            }
            return true;
        }

        /// <summary>
        /// Downloads tasks from a queue with unknown byte ranges. If a task exists in the queue,
        /// it attempts to download the content from the specified URL to the destination file.
        /// It checks if there is enough disk space before downloading. If successful, the task is
        /// moved to the received tasks queue. If the queue is empty or a download is cancelled,
        /// it returns false. Returns true if the download succeeds.
        /// </summary>
        /// <returns>First item is true if the download is successful, second item is true if the download should retry, otherwise false.</returns>
        private async Task<(bool, bool)> DownloadCore()
        {
            try
            {
                await DownloadRanged();
                while (true)
                    if(!await DownloadUnknown())
                        break;
            }
            catch (OperationCanceledException e) when (_cancellationToken.IsCancellationRequested)
            {
                // TODO: handle this exception
                Cancelled();
                return (false, false);
            }
            catch (IOException e)
            {
                // TODO: handle this storage space is run out exception
                return (false, false);
            }
            catch (Exception e)
            {
                // TODO: handle this exception
                return (false, true);
            }
            return (true, false);
        }

        /// <summary>
        /// Executes the core download logic for tasks with either specific or unknown byte ranges.
        /// First, it attempts to download tasks with known byte ranges using the DownloadRanged method.
        /// Then, it continuously tries to download tasks with unknown ranges using the DownloadUnknown method
        /// until there are no more tasks to download or a cancellation is requested.
        /// Handles different exceptions to determine if the download process should be retried.
        /// Returns a tuple indicating the success of the download and whether a retry is recommended.
        /// </summary>
        /// <returns>A tuple containing two boolean values:
        /// - The first item is true if the download is successful, otherwise false.
        /// - The second item is true if the download should be retried, otherwise false.</returns>
        private async Task Download()
        {
            int retryTimes = 0;
            while (retryTimes <= 3)
            {
                retryTimes++;
                (bool isSuccess, bool shouldRetry) = await DownloadCore();
                if (isSuccess)
                    return;
                if (shouldRetry == false)
                {
                    break;
                }
            }

            if (retryTimes > 3)
            {
                // TODO: handle this retry failed exception
            }
        }

        public void Cancel() => _cancellationToken.Cancel();

        private void Merge()
        {
            _cancellationToken.Token.ThrowIfCancellationRequested();

            lock (_receivedTasks)
            {
                FileStream stream = new(_receivedTasks.First().Destination, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                string     previousPath = _receivedTasks.First().Destination;
                foreach (var task in _receivedTasks)
                {
                    var        tempFilePath = $"{task.Destination}.{task.Start}-{task.End}.tmp";
                    FileStream inputStream  = new(tempFilePath, FileMode.Open, FileAccess.Read);
                    if (task.Destination != previousPath)
                    {
                        previousPath = task.Destination;
                        stream.Close();
                        stream = new FileStream(task.Destination, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    inputStream.CopyTo(stream);
                    inputStream.Close();
                    File.Delete(tempFilePath);
                }
            }
        }

        private void Manager()
        {
            List<Task> tasks = [];
            for (int i = 0; i < _concurrency; i++)
                tasks.Add(Task.Run(Download));
            Task.WhenAll(tasks).Wait();
            Merge();
            _isFinished.Set();
        }

        public Downloader Run()
        {
            Task.Run(Manager);
            return this;
        }

        private readonly ManualResetEventSlim _isFinished = new(false);

        public void Wait()
        {
            _isFinished.Wait();
        }

        public Task WaitAsync() => Task.Run(()=> _isFinished.Wait());

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _client.Dispose();
            _isFinished.Dispose();
            _cancellationToken.Dispose();
        }
    }
}