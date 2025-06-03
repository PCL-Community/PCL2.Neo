using System.Net.Http.Headers;
using System.Text.Json;

namespace PCL.Neo.Core.Utils;

#pragma warning disable IL2026 // will fixed by dynamic dependency
public static class Net
{
    public static async Task<TResponse> SendHttpRequestAsync<TResponse>(
        HttpMethod method,
        Uri url,
        object? content = null,
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
                // 在.NET Standard 2.0中没有JsonContent类，使用StringContent替代
                var json = JsonSerializer.Serialize(content);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }
        }

        // 设置授权头
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        // 发送请求
        using var response = await Shared.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // 解析响应
        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<TResponse>(responseJson);
        
        if (result == null)
            throw new ArgumentNullException(nameof(result), "API返回的结果无法解析为指定类型");

        return result;
    }
}