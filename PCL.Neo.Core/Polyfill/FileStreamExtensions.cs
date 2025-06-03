using System;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为FileStream提供.NET Standard 2.0下的异步扩展方法
    /// </summary>
    public static class FileStreamExtensions
    {
        /// <summary>
        /// 异步释放FileStream资源
        /// </summary>
        /// <param name="fileStream">要释放的FileStream</param>
        /// <returns>表示异步操作的任务</returns>
        public static ValueTask DisposeAsync(this FileStream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            // 在.NET Standard 2.0中，我们使用Task.CompletedTask包装同步Dispose操作
            fileStream.Dispose();
            return new ValueTask(Task.CompletedTask);
        }
    }
} 