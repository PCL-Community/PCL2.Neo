using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为CancellationTokenSource提供.NET Standard 2.0下的异步扩展方法
    /// </summary>
    public static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// 异步取消CancellationTokenSource
        /// </summary>
        /// <param name="cancellationTokenSource">要取消的CancellationTokenSource</param>
        /// <returns>表示异步操作的任务</returns>
        public static Task CancelAsync(this CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null)
                throw new ArgumentNullException(nameof(cancellationTokenSource));

            // 在.NET Standard 2.0中，我们使用Task.CompletedTask包装同步Cancel操作
            cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
} 