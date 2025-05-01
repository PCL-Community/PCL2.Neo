using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PCL2.Neo.Helpers;

public static class FileHelper
{
    /// <summary>
    /// 打开系统文件选择框选择一个文件
    /// </summary>
    /// <param name="title">文件选择框的标题</param>
    /// <returns>获得文件的路径</returns>
    public static async Task<string?> SelectFile(string title)
    {
        var storageProvider = App.StorageProvider;
        var files = await storageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = title, AllowMultiple = false });
        if (files.Count < 1)
            return null;
        var file = files[0];
        return file.Path.LocalPath;
    }

    /// <summary>
    /// 检查是否拥有某一文件夹的 I/O 权限。如果文件夹不存在，会返回 False。
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckPermission(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        path = Path.GetFullPath(path);
        try
        {
            var testFile = Path.Combine(path, Path.GetRandomFileName());
            using (File.Create(testFile)) { }

            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

/// <summary>
    /// 从某个 URL 下载并保存文件
    /// </summary>
    /// <param name="uri">下载地址</param>
    /// <param name="localFilePath">本地文件地址</param>
    /// <param name="sha1">文件sha1值</param>
    public static async Task DownloadFileAsync(Uri uri, string localFilePath, string? sha1 = null)
    {
        Console.WriteLine($"Downloading {localFilePath}...");
        ArgumentNullException.ThrowIfNull(uri);
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        await response.Content.CopyToAsync(fileStream);

        if (!string.IsNullOrEmpty(sha1))
        {
            using var sha1Provider = System.Security.Cryptography.SHA1.Create();
            fileStream.Position = 0; // 重置文件流位置
            var computedHash = await sha1Provider.ComputeHashAsync(fileStream);
            var computedHashString = Convert.ToHexStringLower(computedHash);
            if (!string.Equals(computedHashString, sha1, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("文件的 SHA-1 校验失败");
            }
        }
    }

    /// <summary>
    /// 从 MOJANG 官方下载 JRE
    /// </summary>
    /// <param name="platform">平台</param>
    /// <param name="destinationFolder">目标文件夹</param>
    public static async Task FetchJavaOnline(string platform, string destinationFolder)
    {
        Uri metaUrl = new Uri(
            "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json");
        var httpClient = new HttpClient();
        var allJson = await httpClient.GetStringAsync(metaUrl);
        string manifestJson = string.Empty;
        using (var document = JsonDocument.Parse(allJson))
        {
            var root = document.RootElement;
            if (root.TryGetProperty(platform, out JsonElement platformElement) &&
                platformElement.TryGetProperty("java-runtime-gamma", out var gammaArray) &&
                gammaArray.GetArrayLength() > 0 &&
                gammaArray[0].TryGetProperty("manifest", out JsonElement manifestElement) &&
                manifestElement.TryGetProperty("url", out var manifestUriElement))
            {
                var manifestUri = manifestUriElement.GetString();
                if (!string.IsNullOrEmpty(manifestUri))
                {
                    manifestJson = await httpClient.GetStringAsync(manifestUri);
                }
            }

            if (string.IsNullOrEmpty(manifestJson))
            {
                Console.WriteLine("未找到平台 Java 清单");
                return;
            }
        }

        var manifest = JsonNode.Parse(manifestJson)?.AsObject();
        if (manifest == null || !manifest.TryGetPropertyValue("files", out var filesNode))
        {
            Console.WriteLine("无效的清单文件");
            return;
        }

        var files = filesNode!.AsObject();
        var tasks = new List<Task>(files.Count);
        foreach ((string filePath, JsonNode? value) in files)
        {
            var fileInfo = value!.AsObject();
            if (!fileInfo.TryGetPropertyValue("type", out var typeNode) || typeNode!.ToString() != "file")
                continue;
            if (!fileInfo.TryGetPropertyValue("downloads", out var downloadsNode))
                continue;
            var downloads = downloadsNode!.AsObject();
            if (!downloads.TryGetPropertyValue("raw", out var rawNode))
                continue;
            var raw = rawNode!.AsObject();
            string url = raw["url"]!.ToString();
            string sha1 = raw["sha1"]!.ToString();
            string localFilePath = Path.Combine(destinationFolder,
                filePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);
            tasks.Add(DownloadFileAsync(new Uri(url), localFilePath, sha1));
        }
        await Task.WhenAll(tasks);
    }
}