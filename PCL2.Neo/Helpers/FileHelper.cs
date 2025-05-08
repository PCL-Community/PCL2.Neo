using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using PCL2.Neo.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PCL2.Neo.Helpers;

/// <summary>
/// 一些文件操作和下载请求之类的
/// </summary>
public static class FileHelper
{
    public static readonly HttpClient HttpClient = new();

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
                        bool isSha1Match = await fileStream.CheckSha1(sha1);
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
    private static async Task<bool> CheckSha1(this FileStream fileStream, string sha1)
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
    public static void SetFileExecutableUnix(this string path)
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
    private static FileStream? DecompressLZMA(this FileStream inStream, string outputFile)
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
    public static async Task DownloadAndDeCompressFileAsync(Uri uri, string localFilePath, string sha1Raw,
        string sha1Lzma, CancellationToken cancellationToken = default)
    {
        var stream = await DownloadFileAsync(uri, localFilePath + ".lzma", sha1Lzma, true,
            cancellationToken: cancellationToken);
        if (stream != null)
        {
            var outStream = stream.DecompressLZMA(localFilePath);
            if (outStream == null)
            {
                Console.WriteLine("outStream 为空");
                return;
            }

            var match = await outStream.CheckSha1(sha1Raw);
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
}