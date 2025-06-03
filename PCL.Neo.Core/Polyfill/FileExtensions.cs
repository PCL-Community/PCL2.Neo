using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// 为.NET Standard 2.0提供文件操作的兼容方法
    /// </summary>
    public static class FilePolyfill
    {
        /// <summary>
        /// 异步读取文本文件的所有内容
        /// </summary>
        public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.ReadAllText(path), cancellationToken);
        }

        /// <summary>
        /// 异步读取文本文件的所有内容
        /// </summary>
        public static Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.ReadAllText(path, encoding), cancellationToken);
        }

        /// <summary>
        /// 异步将文本写入文件
        /// </summary>
        public static Task WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.WriteAllText(path, contents), cancellationToken);
        }

        /// <summary>
        /// 异步将文本写入文件
        /// </summary>
        public static Task WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.WriteAllText(path, contents, encoding), cancellationToken);
        }

        /// <summary>
        /// 异步将字节数组写入文件
        /// </summary>
        public static Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => File.WriteAllBytes(path, bytes), cancellationToken);
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供路径操作的兼容方法
    /// </summary>
    public static class PathPolyfill
    {
        /// <summary>
        /// 检查路径是否存在
        /// </summary>
        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供Process的扩展方法
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// 异步等待进程退出
        /// </summary>
        public static Task WaitForExitAsync(this System.Diagnostics.Process process, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);
            
            if (process.HasExited)
                return Task.CompletedTask;
            
            if (cancellationToken != default)
                cancellationToken.Register(() => tcs.TrySetCanceled());
            
            return tcs.Task;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供Task的扩展方法
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// 异步等待任务完成，带超时
        /// </summary>
        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            
            if (completedTask == timeoutTask)
                throw new TimeoutException("The operation timed out.");
            
            return await task;
        }

        /// <summary>
        /// 异步等待任务完成，带超时
        /// </summary>
        public static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutTask = Task.Delay(timeout, cancellationToken);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            
            if (completedTask == timeoutTask)
                throw new TimeoutException("The operation timed out.");
            
            await task;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供CancellationTokenSource的扩展方法
    /// </summary>
    public static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// 异步取消操作
        /// </summary>
        public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource)
        {
            return Task.Run(() => cancellationTokenSource.Cancel());
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供FileStream的扩展方法
    /// </summary>
    public static class FileStreamExtensions
    {
        /// <summary>
        /// 异步释放资源
        /// </summary>
        public static ValueTask DisposeAsync(this FileStream fileStream)
        {
            fileStream.Dispose();
            return new ValueTask();
        }
    }
} 