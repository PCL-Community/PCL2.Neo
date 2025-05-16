using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.RegularExpressions;

namespace PCL.Neo.Models.User;

/// <summary>
/// 离线用户信息
/// </summary>
public partial class OfflineUser : ObservableObject
{
    private static readonly string DefaultUUID = "00000000-0000-0000-0000-000000000000";
    
    [ObservableProperty]
    private string _username = "Player";
    
    [ObservableProperty]
    private string _uuid = DefaultUUID;
    
    [ObservableProperty] 
    private string _skinPath = string.Empty;
    
    [ObservableProperty]
    private UserType _type = UserType.Offline;
    
    [ObservableProperty]
    private string _accessToken = string.Empty;
    
    [ObservableProperty]
    private string _avatarUrl = string.Empty;
    
    public string UUID 
    { 
        get => _uuid; 
        set => SetProperty(ref _uuid, value); 
    }
    
    public OfflineUser() : base()
    {
        Type = UserType.Offline;
    }
    
    public OfflineUser(string username) : base()
    {
        Username = username;
        UUID = GenerateOfflineUUID(username);
        Type = UserType.Offline;
        AccessToken = Guid.NewGuid().ToString();
        AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png";
    }
    
    /// <summary>
    /// 根据用户名生成UUID
    /// </summary>
    public static string GenerateOfflineUUID(string username)
    {
        // 检查用户名是否符合规范
        if (string.IsNullOrEmpty(username) || !IsValidUsername(username))
        {
            return DefaultUUID;
        }
        
        // 生成随机但固定的UUID（基于用户名）
        var guid = new Guid(MurmurHash3.Hash(username));
        return guid.ToString();
    }
    
    /// <summary>
    /// 验证用户名是否符合Minecraft规范
    /// </summary>
    public static bool IsValidUsername(string username)
    {
        // Minecraft用户名规则：3-16个字符，只允许字母、数字和下划线
        return !string.IsNullOrEmpty(username) && 
               username.Length >= 3 && 
               username.Length <= 16 && 
               UsernameRegex().IsMatch(username);
    }
    
    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex UsernameRegex();

    public static OfflineUser CreateOfflineUser(string username)
    {
        return new OfflineUser(username);
    }
}

/// <summary>
/// MurmurHash3算法实现，用于生成UUID
/// </summary>
internal static class MurmurHash3
{
    public static byte[] Hash(string str)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);
        const uint seed = 144;
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;
        
        uint h1 = seed;
        
        uint k1 = 0;
        
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
        
        // 处理剩余字节
        k1 = 0;
        
        switch (len & 3)
        {
            case 3:
                k1 ^= (uint)(bytes[i + 2] & 0xFF) << 16;
                goto case 2;
            case 2:
                k1 ^= (uint)(bytes[i + 1] & 0xFF) << 8;
                goto case 1;
            case 1:
                k1 ^= (uint)(bytes[i] & 0xFF);
                k1 *= c1;
                k1 = RotateLeft(k1, 15);
                k1 *= c2;
                h1 ^= k1;
                break;
        }
        
        // 最终混淆
        h1 ^= (uint)len;
        h1 = Fmix(h1);
        
        // 转换为16字节
        byte[] result = new byte[16];
        BitConverter.GetBytes(h1).CopyTo(result, 0);
        BitConverter.GetBytes(h1 ^ seed).CopyTo(result, 4);
        BitConverter.GetBytes(seed ^ (h1 >> 16)).CopyTo(result, 8);
        BitConverter.GetBytes(seed ^ (h1 << 8)).CopyTo(result, 12);
        
        return result;
    }
    
    private static uint RotateLeft(uint x, int r)
    {
        return (x << r) | (x >> (32 - r));
    }
    
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