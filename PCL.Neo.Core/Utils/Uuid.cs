using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Utils;

public static class Uuid
{
    private const string DefaultUuid = "00000000-0000-0000-0000-000000000000";

    public static string GenerateOfflineUuid(string username)
    {
        if (string.IsNullOrEmpty(username) || !IsValidUsername(username))
            return DefaultUuid;
        var guid = new Guid(MurmurHash3.Hash(username));
        return guid.ToString();
    }

    public static bool IsValidUsername(string username)
    {
        return !string.IsNullOrEmpty(username) &&
               username.Length >= 3 &&
               username.Length <= 16 &&
               Regex.IsMatch(username, "^[a-zA-Z0-9_]+$");
    }

    // MurmurHash3算法实现
    private static class MurmurHash3
    {
        public static byte[] Hash(string str)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(str);
            const uint seed = 144;
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;
            uint h1 = seed;
            uint k1;
            int len = bytes.Length;
            int i = 0;
            for (; i + 4 <= len; i += 4)
            {
                k1 = (uint)((bytes[i] & 0xFF) |
                    ((bytes[i + 1] & 0xFF) << 8) |
                    ((bytes[i + 2] & 0xFF) << 16) |
                    ((bytes[i + 3] & 0xFF) << 24));
                k1 *= c1;
                k1 = RotateLeft(k1, 15);
                k1 *= c2;
                h1 ^= k1;
                h1 = RotateLeft(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }
            k1 = 0;
            switch (len & 3)
            {
                case 3: k1 ^= (uint)(bytes[i + 2] & 0xFF) << 16; goto case 2;
                case 2: k1 ^= (uint)(bytes[i + 1] & 0xFF) << 8; goto case 1;
                case 1:
                    k1 ^= (uint)(bytes[i] & 0xFF);
                    k1 *= c1;
                    k1 = RotateLeft(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
            }
            h1 ^= (uint)len;
            h1 = Fmix(h1);
            byte[] result = new byte[16];
            BitConverter.GetBytes(h1).CopyTo(result, 0);
            BitConverter.GetBytes(h1 ^ seed).CopyTo(result, 4);
            BitConverter.GetBytes(seed ^ (h1 >> 16)).CopyTo(result, 8);
            BitConverter.GetBytes(seed ^ (h1 << 8)).CopyTo(result, 12);
            return result;
        }
        private static uint RotateLeft(uint x, int r) => (x << r) | (x >> (32 - r));
        private static uint Fmix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}