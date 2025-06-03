using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为.NET Standard 2.0提供Process的扩展方法
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// 异步等待进程退出
        /// </summary>
        /// <param name="process">进程</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>等待任务</returns>
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;

            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default)
                cancellationToken.Register(() => tcs.TrySetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
} 