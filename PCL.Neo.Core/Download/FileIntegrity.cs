using System.Security.Cryptography;

namespace PCL.Neo.Core.Download;

public class FileIntegrityException(string? msg = null) : Exception(msg);

public record FileIntegrity(
    long ExpectedSize = -1,
    HashAlgorithm? HashAlgorithm = null,
    string Hash = "")
{
    private byte[] HashBytes => StringToByteArray(Hash);

    // .NET Standard 2.0兼容的十六进制字符串转换方法
    private static byte[] StringToByteArray(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            return Array.Empty<byte>();

        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public async Task<bool> VerifyAsync(string filepath, CancellationToken token = default)
    {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await VerifyAsync(fs, token);
    }

    public async Task<bool> VerifyAsync(Stream stream, CancellationToken token = default)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException($"{nameof(stream)} is not readable");
        if (ExpectedSize >= 0)
        {
            if (!stream.CanSeek)
                throw new InvalidOperationException(
                    $"{nameof(ExpectedSize)} >= 0 but {nameof(stream)} is not seekable");
            if (stream.Length != ExpectedSize)
                return false;
        }

        if (string.IsNullOrEmpty(Hash)) // skip hash check if no hash specified
            return true;

        var hasher = HashAlgorithm ?? SHA1.Create(); // default to sha1 if not specified

        stream.Seek(0, SeekOrigin.Begin);
        byte[] hash = hasher.ComputeHash(stream);
        return hash.SequenceEqual(HashBytes);
    }

    public bool Verify(string filepath) => VerifyAsync(filepath).Result;
    public bool Verify(Stream stream) => VerifyAsync(stream).Result;

    public bool Verify(byte[] data)
    {
        if (ExpectedSize >= 0 && data.Length != this.ExpectedSize)
            return false;
        if (string.IsNullOrEmpty(Hash)) // skip hash check if no hash specified
            return true;
        var hasher = HashAlgorithm ?? SHA1.Create();
        return hasher.ComputeHash(data).SequenceEqual(HashBytes);
    }
}