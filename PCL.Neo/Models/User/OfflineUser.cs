using CommunityToolkit.Mvvm.ComponentModel;
using System;
using PCL.Neo.Utils;
using System.Text.RegularExpressions;

namespace PCL.Neo.Models.User;

/// <summary>
/// 离线用户信息
/// </summary>
public partial class OfflineUser : ObservableObject
{
    [ObservableProperty] private string _username = "Player";

    [ObservableProperty] private string _uuid = UuidUtils.DefaultUuid;

    [ObservableProperty] private string _skinPath = string.Empty;

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

    public OfflineUser() => Type = UserType.Offline;

    public OfflineUser(string username)
    {
        Username = username;
        UUID = UuidUtils.GenerateOfflineUuid(username);
        Type = UserType.Offline;
        AccessToken = Guid.NewGuid().ToString();
        AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png";
    }

    public static OfflineUser CreateOfflineUser(string username) => new(username);
}