using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Utils;

#pragma warning disable IL2026 // will fixed by dynamic dependency
public static class Net
{
    public static HttpClient SharedHttpClient = new();

    public static async Task<TResponse> SendHttpRequestAsync<TResponse>(
        HttpMethod method,
        Uri url,
        object? content = null,
        JsonSerializerOptions? jsonOptions = null,
        string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(method, url);

        // 设置请求体
        if (content != null)
        {
            if (content is FormUrlEncodedContent formContent)
            {
                request.Content = formContent;
            }
            else
            {
                request.Content = JsonContent.Create(content, options: jsonOptions);
            }
        }

        // 设置授权头
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        // 发送请求
        using var response = await SharedHttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // 解析响应
        var result = await response.Content.ReadFromJsonAsync<TResponse>(jsonOptions);
        ArgumentNullException.ThrowIfNull(result);

        return result;
    }
}