using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为Task提供.NET Standard 2.0下的异步扩展方法
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// 异步等待任务完成，支持超时
        /// </summary>
        /// <typeparam name="T">任务结果类型</typeparam>
        /// <param name="task">要等待的任务</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>原始任务的结果</returns>
        /// <exception cref="TimeoutException">如果任务在指定的超时时间内未完成</exception>
        public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // 重新等待以传播异常
                }
                else
                {
                    throw new TimeoutException("操作已超时");
                }
            }
        }

        /// <summary>
        /// 异步等待任务完成，支持超时
        /// </summary>
        /// <param name="task">要等待的任务</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="TimeoutException">如果任务在指定的超时时间内未完成</exception>
        public static async Task WaitAsync(this Task task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;  // 重新等待以传播异常
                }
                else
                {
                    throw new TimeoutException("操作已超时");
                }
            }
        }

        /// <summary>
        /// 异步等待任务完成，支持取消令牌
        /// </summary>
        /// <typeparam name="T">任务结果类型</typeparam>
        /// <param name="task">要等待的任务</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>原始任务的结果</returns>
        /// <exception cref="OperationCanceledException">如果取消令牌被触发</exception>
        public static async Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return await task;

            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => tcs.TrySetResult(true)))
            {
                var completedTask = await Task.WhenAny(task, tcs.Task);
                if (completedTask == task)
                    return await task;  // 重新等待以传播异常
                
                cancellationToken.ThrowIfCancellationRequested();
                // 不应该到达这里
                throw new OperationCanceledException();
            }
        }

        /// <summary>
        /// 异步等待任务完成，支持取消令牌
        /// </summary>
        /// <param name="task">要等待的任务</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>表示异步操作的任务</returns>
        /// <exception cref="OperationCanceledException">如果取消令牌被触发</exception>
        public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
            {
                await task;
                return;
            }

            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => tcs.TrySetResult(true)))
            {
                var completedTask = await Task.WhenAny(task, tcs.Task);
                if (completedTask == task)
                {
                    await task;  // 重新等待以传播异常
                    return;
                }
                
                cancellationToken.ThrowIfCancellationRequested();
                // 不应该到达这里
                throw new OperationCanceledException();
            }
        }
    }
} 