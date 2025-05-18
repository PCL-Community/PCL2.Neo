using System.Security.Cryptography;

namespace PCL.Neo.Core.Download;

public record FileIntegrity(
    long ExpectedSize = -1,
    HashAlgorithm? HashAlgorithm = null,
    string Hash = "")
{
    private byte[] HashBytes => Convert.FromHexString(Hash);

    public bool Verify(string filepath)
    {
        using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Verify(fs);
    }

    public bool Verify(Stream stream)
    {
        if (!stream.CanRead)
            throw new InvalidOperationException($"{nameof(stream)} is not readable");
        if (ExpectedSize >= 0)
        {
            if (!stream.CanSeek)
                throw new InvalidOperationException($"{nameof(ExpectedSize)} >= 0 but {nameof(stream)} is not seekable");
            if (stream.Length != ExpectedSize)
                return false;
        }

        if (string.IsNullOrEmpty(Hash)) // skip hash check if no hash specified
            return true;

        var hasher = HashAlgorithm ?? SHA1.Create(); // default to sha1 if not specified

        stream.Seek(0, SeekOrigin.Begin);
        return hasher.ComputeHash(stream).SequenceEqual(HashBytes);
    }

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