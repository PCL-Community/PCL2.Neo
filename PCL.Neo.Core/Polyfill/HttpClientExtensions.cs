using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为HttpClient提供.NET Standard 2.0下的异步扩展方法
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// 发送GET请求并以字符串形式返回响应内容，支持取消令牌
        /// </summary>
        /// <param name="client">HttpClient实例</param>
        /// <param name="requestUri">请求URI</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应内容字符串</returns>
        public static async Task<string> GetStringAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            using (var response = await client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 发送GET请求并以字符串形式返回响应内容，支持取消令牌
        /// </summary>
        /// <param name="client">HttpClient实例</param>
        /// <param name="requestUri">请求URI字符串</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应内容字符串</returns>
        public static Task<string> GetStringAsync(this HttpClient client, string requestUri, CancellationToken cancellationToken)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return client.GetStringAsync(new Uri(requestUri), cancellationToken);
        }
    }
} 