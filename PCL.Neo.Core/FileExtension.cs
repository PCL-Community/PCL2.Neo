using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace PCL.Neo.Core;

/// <summary>
/// 一些文件操作和下载请求之类的
/// </summary>
public static class FileExtension
{
    /// <summary>
    /// 校验文件流与SHA-1是否匹配
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="sha1">SHA-1</param>
    /// <returns>是否匹配</returns>
    public static Task<bool> CheckSha1(this FileStream fileStream, string sha1)
    {
        using var sha1Provider = System.Security.Cryptography.SHA1.Create();
        fileStream.Position = 0; // 重置文件流位置

        var computedHash = sha1Provider.ComputeHash(fileStream);
        var computedHashString = ToHexStringLower(computedHash);

        return Task.FromResult(string.Equals(computedHashString, sha1, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 将字节数组转换为小写十六进制字符串
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns>小写十六进制字符串</returns>
    private static string ToHexStringLower(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// 在 Unix 系统中给予可执行文件运行权限
    /// </summary>
    /// <param name="path">文件路径</param>
    [SupportedOSPlatform(nameof(OSPlatform.OSX))]
    [SupportedOSPlatform(nameof(OSPlatform.Linux))]
    public static void SetFileExecutableUnix(this string path)
    {
        // check is system windows
        if (SystemUtils.Os is SystemUtils.RunningOs.Windows)
        {
            return;
        }

        try
        {
            // 在.NET Standard 2.0中，UnixFileMode和相关方法不可用
            // 使用Process启动chmod命令来设置权限
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
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
    public static FileStream? DecompressLzma(this FileStream inStream, string outputFile)
    {
        inStream.Position = 0;

        var outStream        = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite);
        var decodeProperties = new byte[5];
        var debugPos         = inStream.Read(decodeProperties, 0, 5);

        Debug.Assert(debugPos == 5);

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
}