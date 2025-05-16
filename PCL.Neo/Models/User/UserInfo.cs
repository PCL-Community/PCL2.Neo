using System;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Core.Utils;
using PCL.Neo.Utils;

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
    public required BaseAccount Account { get; set; }

    // UI/本地管理相关属性
    public string AvatarUrl { get; set; } = string.Empty;
    public bool Selected { get; set; }
    public DateTime LastUsed { get; set; } = DateTime.Now;
    public DateTime AddedTime { get; set; } = DateTime.Now;
    public DateTimeOffset? AuthExpireTime { get; set; }

    // 只读属性，便于UI绑定
    public string Username => Account.UserName;
    public string UUID => Account.Uuid;

    public UserType Type => Account switch
    {
        MsaAccount => UserType.Microsoft,
        OfflineAccount => UserType.Offline,
        YggdrasilAccount => UserType.Authlib,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string ServerUrl { get; set; }


    // 工厂方法示例
    public static UserInfo CreateOfflineUser(OfflineAccount account)
    {
        return new UserInfo { Account = account, AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png" };
    }

    public static UserInfo CreateOfflineUser(string userName)
    {
        var account = new OfflineAccount()
        {
            Capes = [],
            Skins = [],
            UserName = "Player",
            UserProperties = string.Empty,
            Uuid = UuidUtils.GenerateOfflineUuid("Player")
        };

        return new UserInfo { Account = account, AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png" };
    }

    // 检查账户是否过期
    public bool IsExpired() =>
        Type != UserType.Offline && AuthExpireTime.HasValue && DateTime.Now > AuthExpireTime.Value;

    public string GetDisplayName() => Username;
    public string GetInitial() => string.IsNullOrEmpty(Username) ? "?" : Username[..1].ToUpper();

    public string GetUserTypeText() =>
        Account.UserType switch
        {
            UserTypeConstants.Offline => "离线账户",
            UserTypeConstants.Msa => "微软账户",
            UserTypeConstants.Yggdrasil => "外置登录",
            _ => "未知账户"
        };
}