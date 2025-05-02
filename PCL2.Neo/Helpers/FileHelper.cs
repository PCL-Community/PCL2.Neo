using Avalonia.Platform.Storage;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PCL2.Neo.Helpers;

/// <summary>
/// 一些文件操作和下载请求之类的
/// </summary>
public static class FileHelper
{
    private static readonly HttpClient HttpClient = new();

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
    /// <param name="path">文件夹路径</param>
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
    /// <param name="passStreamDown">是否向外传递下载的文件流</param>
    /// <param name="maxRetries">最大重试次数</param>
    /// <param name="cancellationToken">用于取消</param>
    /// <returns>向外传递的文件流</returns>
    public static async Task<FileStream?> DownloadFileAsync(
        Uri uri, string localFilePath, string? sha1 = null, bool passStreamDown = false, int maxRetries = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Console.WriteLine($"Downloading {localFilePath}...");
        int attempt = 0;
        const int baseDelayMs = 500;
        while (true)
        {
            try
            {
                using var response =
                    await HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                try
                {
                    await response.Content.CopyToAsync(fileStream, cancellationToken);
                    if (!string.IsNullOrEmpty(sha1))
                    {
                        fileStream.Position = 0;
                        bool isSha1Match = await CheckSha1(fileStream, sha1);
                        if (!isSha1Match)
                            throw new IOException($"SHA-1 mismatch for file: {localFilePath}");
                    }

                    if (passStreamDown)
                    {
                        fileStream.Position = 0;
                        return fileStream;
                    }

                    fileStream.Close();
                    return null;
                }
                catch
                {
                    fileStream.Dispose();
                    throw;
                }
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                attempt++;
                int delay = baseDelayMs * (1 << (attempt - 1)); // 500, 1000, 2000...
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}. Retrying in {delay} ms...");
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// 校验文件流与SHA-1是否匹配
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="sha1">SHA-1</param>
    /// <returns>是否匹配</returns>
    private static async Task<bool> CheckSha1(FileStream fileStream, string sha1)
    {
        using var sha1Provider = System.Security.Cryptography.SHA1.Create();
        fileStream.Position = 0; // 重置文件流位置
        var computedHash = await sha1Provider.ComputeHashAsync(fileStream);
        var computedHashString = Convert.ToHexStringLower(computedHash);
        return string.Equals(computedHashString, sha1, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 在 Unix 系统中给予可执行文件运行权限
    /// </summary>
    /// <param name="path">文件路径</param>
    [SupportedOSPlatform(nameof(OSPlatform.OSX))]
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    private static void SetFileExecutableUnix(string path)
    {
        if (Const.Os is Const.RunningOs.Windows) return;
        try
        {
            var currentMode = File.GetUnixFileMode(path);
            var newMode = currentMode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute |
                          UnixFileMode.OtherExecute;
            File.SetUnixFileMode(path, newMode);
        }
        catch (Exception e)
        {
            Console.WriteLine($"无法设置可执行权限：{e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 从某个地方抄来的很像 C 语言风格的解压 LZMA 压缩算法的函数
    /// </summary>
    /// <param name="inStream">被压缩的文件流</param>
    /// <param name="outputFile">输出文件路径</param>
    /// <returns>解压后的文件流</returns>
    private static FileStream? DecompressFile(FileStream inStream, string outputFile)
    {
        inStream.Position = 0;
        var outStream = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite);
        byte[] decodeProperties = new byte[5];
        int n = inStream.Read(decodeProperties, 0, 5);
        Debug.Assert(n == 5);
        SevenZip.Compression.LZMA.Decoder decoder = new();
        decoder.SetDecoderProperties(decodeProperties);
        long outSize = 0;
        for (int i = 0; i < 8; i++)
        {
            int v = inStream.ReadByte();
            if (v < 0)
            {
                Console.WriteLine("read outSize error.");
                return null;
            }

            outSize |= (long)(byte)v << (8 * i);
        }

        long compressedSize = inStream.Length - inStream.Position;
        decoder.Code(inStream, outStream, compressedSize, outSize, null);
        inStream.Close();
        return outStream;
    }

    /// <summary>
    /// 整合函数：下载并解压，然后删去原压缩文件
    /// </summary>
    private static async Task DownloadAndDeCompressFileAsync(Uri uri, string localFilePath, string sha1Raw,
        string sha1Lzma, CancellationToken cancellationToken = default)
    {
        var stream = await DownloadFileAsync(uri, localFilePath + ".lzma", sha1Lzma, true,
            cancellationToken: cancellationToken);
        if (stream != null)
        {
            var outStream = DecompressFile(stream, localFilePath);
            if (outStream == null)
            {
                Console.WriteLine("outStream 为空");
                return;
            }

            var match = await CheckSha1(outStream, sha1Raw);
            if (!match)
            {
                Console.WriteLine("解压后的文件SHA-1与源提供的不匹配");
                return;
            }

            stream.Close();
            outStream.Close();
        }

        File.Delete(localFilePath + ".lzma");
    }

    /// <summary>
    /// 从 MOJANG 官方下载 JRE
    /// </summary>
    /// <param name="platform">平台</param>
    /// <param name="destinationFolder">目标文件夹</param>
    /// <param name="progressCallback">显示进度的回调函数</param>
    /// <param name="cancellationToken">用于中断下载</param>
    /// <param name="version">要下载的版本，有α、β、γ、δ等</param>
    /// <returns>如果未成功下载为null，成功下载则为java可执行文件所在的目录</returns>
    public static async Task<string?> FetchJavaOnline(string platform, string destinationFolder,
        Java.MojangJavaVersion version,
        Action<int, int>? progressCallback = null, CancellationToken cancellationToken = default)
    {
        Uri metaUrl = new(
            "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json");
        var allJson = await HttpClient.GetStringAsync(metaUrl, cancellationToken);
        string manifestJson = string.Empty;
        using (var document = JsonDocument.Parse(allJson))
        {
            var root = document.RootElement;
            if (root.TryGetProperty(platform, out JsonElement platformElement) &&
                platformElement.TryGetProperty(version.Value, out var gammaArray) &&
                gammaArray.GetArrayLength() > 0 &&
                gammaArray[0].TryGetProperty("manifest", out JsonElement manifestElement) &&
                manifestElement.TryGetProperty("url", out var manifestUriElement))
            {
                var manifestUri = manifestUriElement.GetString();
                if (!string.IsNullOrEmpty(manifestUri))
                {
                    manifestJson = await HttpClient.GetStringAsync(manifestUri, cancellationToken);
                }
            }

            if (string.IsNullOrEmpty(manifestJson))
            {
                Console.WriteLine("未找到平台 Java 清单");
                return null;
            }
        }

        var manifest = JsonNode.Parse(manifestJson)?.AsObject();
        if (manifest == null || !manifest.TryGetPropertyValue("files", out var filesNode))
        {
            Console.WriteLine("无效的清单文件");
            return null;
        }

        var files = filesNode!.AsObject();
        var tasks = new List<Task>(files.Count);
        var executableFiles = new List<string>(files.Count);
        foreach ((string filePath, JsonNode? value) in files)
        {
            var fileInfo = value!.AsObject();
            if (!fileInfo.TryGetPropertyValue("type", out var typeNode) || typeNode!.ToString() != "file")
                continue;
            if (!fileInfo.TryGetPropertyValue("downloads", out var downloadsNode))
                continue;
            var downloads = downloadsNode!.AsObject();
            bool isExecutable = fileInfo.TryGetPropertyValue("executable", out var execNode) &&
                                execNode!.GetValue<bool>();
            string? urlRaw = null, sha1Raw = null, urlLzma = null, sha1Lzma = null;
            if (downloads.TryGetPropertyValue("raw", out var rawNode))
            {
                var raw = rawNode!.AsObject();
                urlRaw = raw["url"]!.ToString();
                sha1Raw = raw["sha1"]!.ToString();
            }

            Debug.Assert(rawNode != null && !string.IsNullOrEmpty(urlRaw) && !string.IsNullOrEmpty(sha1Raw),
                "rawNode 不存在");

            if (downloads.TryGetPropertyValue("lzma", out var lzmaNode))
            {
                var lzma = lzmaNode!.AsObject();
                urlLzma = lzma["url"]!.ToString();
                sha1Lzma = lzma["sha1"]!.ToString();
            }

            string localFilePath = Path.Combine(destinationFolder,
                filePath.Replace("/", Const.Sep.ToString()));
            if (isExecutable) executableFiles.Add(localFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath)!);
            // 有的文件有LZMA压缩但是有的 tm 没有，尼玛搞了个解压缩发现文件少了几个
            // 要分类讨论，sb MOJANG
            if (lzmaNode != null && !string.IsNullOrEmpty(urlLzma))
                tasks.Add(DownloadAndDeCompressFileAsync(new Uri(urlLzma), localFilePath, sha1Raw, sha1Lzma!,
                    cancellationToken));
            else
                tasks.Add(DownloadFileAsync(new Uri(urlRaw), localFilePath, sha1Raw,
                    cancellationToken: cancellationToken));
        }

        int completed = 0;
        int total = tasks.Count;
        while (total - completed > 0)
        {
            var finishedTask = await Task.WhenAny(tasks);
            progressCallback?.Invoke(++completed, total);
            try { await finishedTask; }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        await Task.WhenAll(tasks);

#pragma warning disable CA1416
        if (Const.Os is not Const.RunningOs.Windows)
        {
            foreach (string executableFile in executableFiles)
            {
                if (string.IsNullOrEmpty(executableFile) || !File.Exists(executableFile))
                    throw new FileNotFoundException();
                SetFileExecutableUnix(executableFile);
            }
        }
#pragma warning restore CA1416
        var targetFolder = Const.Os switch
        {
            Const.RunningOs.MacOs => Path.Combine(destinationFolder, "jre.bundle/Contents/Home/bin"),
            Const.RunningOs.Linux => Path.Combine(destinationFolder, "bin"),
            Const.RunningOs.Windows => Path.Combine(destinationFolder, "bin"),
            Const.RunningOs.Unknown => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
        return targetFolder;
    }
}