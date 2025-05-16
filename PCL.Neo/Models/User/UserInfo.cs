using System;

namespace PCL.Neo.Models.User;

public enum UserType
{
    Offline,
    Microsoft,
    Authlib
}

public class UserInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string UUID { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public UserType Type { get; set; } = UserType.Offline;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public DateTime LastUsed { get; set; } = DateTime.Now;
    public DateTime AddedTime { get; set; } = DateTime.Now;
    public DateTime? AuthExpireTime { get; set; }
    
    // Microsoft账户特有属性
    public string RefreshToken { get; set; } = string.Empty;
    public bool HasCape { get; set; }
    
    // Authlib账户特有属性
    public string ServerUrl { get; set; } = string.Empty;
    
    // 创建离线账户
    public static UserInfo CreateOfflineUser(string username)
    {
        return new UserInfo
        {
            Username = username,
            UUID = GenerateOfflineUUID(username),
            Type = UserType.Offline,
            AccessToken = Guid.NewGuid().ToString(),
            AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png"
        };
    }
    
    // 根据用户名生成离线UUID
    private static string GenerateOfflineUUID(string username)
    {
        // 离线模式使用用户名的MD5作为UUID
        using var md5 = System.Security.Cryptography.MD5.Create();
        var inputBytes = System.Text.Encoding.UTF8.GetBytes($"OfflinePlayer:{username}");
        var hashBytes = md5.ComputeHash(inputBytes);
        
        // 设置UUID版本 (版本3 = MD5)
        hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);
        // 设置UUID变体
        hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);
        
        // 转换为UUID字符串格式
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
    
    // 检查账户是否过期
    public bool IsExpired()
    {
        return Type != UserType.Offline && AuthExpireTime.HasValue && DateTime.Now > AuthExpireTime.Value;
    }
    
    // 获取用户显示名
    public string GetDisplayName()
    {
        return Username;
    }
    
    // 获取用户首字母
    public string GetInitial()
    {
        if (string.IsNullOrEmpty(Username))
            return "?";
            
        return Username.Substring(0, 1).ToUpper();
    }
    
    // 获取用户类型显示文本
    public string GetUserTypeText()
    {
        return Type switch
        {
            UserType.Offline => "离线账户",
            UserType.Microsoft => "微软账户",
            UserType.Authlib => "外置登录",
            _ => "未知账户"
        };
    }
} 