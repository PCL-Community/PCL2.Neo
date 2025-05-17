using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PCL.Neo.Core.Service.Accounts.Exceptions;
using PCL.Neo.Models.User;
using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Utils;

namespace PCL.Neo.Services;

/// <summary>
/// 用户管理服务
/// </summary>
public class UserService : ObservableObject
{
    private const string UserConfigFile = "users.json";
    private readonly string _configPath;
    private readonly StorageService _storageService;
    private readonly IMicrosoftAuthService _microsoftAuthService;

    private ObservableCollection<UserInfo> _users = [];
    public ReadOnlyObservableCollection<UserInfo> Users { get; }
    
    private UserInfo? _currentUser;
    public UserInfo? CurrentUser
    {
        get => _currentUser;
        private set => SetProperty(ref _currentUser, value);
    }

    public event Action<UserInfo?>? CurrentUserChanged;

    public UserService(StorageService storageService, IMicrosoftAuthService microsoftAuthService)
    {
        _storageService = storageService;
        _microsoftAuthService = microsoftAuthService;
        _configPath = Path.Combine(StorageService.AppDataDirectory, UserConfigFile);
        Users = new ReadOnlyObservableCollection<UserInfo>(_users);
        
        // 初始化用户列表
        _ = InitializeAsync();
    }
    
    /// <summary>
    /// 初始化用户服务
    /// </summary>
    public async Task InitializeAsync()
    {
        // 加载用户列表
        await LoadUsersAsync();
        
        // 如果没有用户，创建默认用户
        if (_users.Count == 0)
        {
            var defaultUser = UserInfo.CreateOfflineUser("Player");
            defaultUser.Selected = true;
            _users.Add(defaultUser);
            await SaveUsersAsync();
        }
        
        // 设置当前用户
        CurrentUser = _users.FirstOrDefault(u => u.Selected) ?? _users.First();
    }
    
    /// <summary>
    /// 加载用户列表
    /// </summary>
    public async Task LoadUsersAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                var users = JsonSerializer.Deserialize<List<UserInfo>>(json);

                if (users is { Count: > 0 })
                {
                    _users.Clear();
                    foreach (var user in users)
                    {
                        _users.Add(user);
                    }

                    // 设置当前用户为列表中的第一个用户
                    CurrentUser = _users.FirstOrDefault();
                    return;
                }
            }
            
            // 如果没有用户，创建一个默认用户
            _users.Clear();
            var defaultUser = UserInfo.CreateOfflineUser("Player");
            _users.Add(defaultUser);
            CurrentUser = defaultUser;
            
            // 保存默认用户
            await SaveUsersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载用户配置失败: {ex.Message}");
            
            // 出错时创建默认用户
            _users.Clear();
            var defaultUser = UserInfo.CreateOfflineUser("Player");
            _users.Add(defaultUser);
            CurrentUser = defaultUser;
        }
    }
    
    /// <summary>
    /// 保存用户列表
    /// </summary>
    public async Task SaveUsersAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_users.ToList());
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存用户配置失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 添加新用户
    /// </summary>
    public async Task<UserInfo> AddUserAsync(string username)
    {
        // 验证用户名
        if (!OfflineUser.IsValidUsername(username))
        {
            throw new ArgumentException("无效的用户名，用户名必须为3-16个字符，且只能包含字母、数字和下划线。");
        }
        
        // 检查是否已存在同名用户
        if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"用户 {username} 已存在。");
        }
        
        // 创建新用户
        var user = UserInfo.CreateOfflineUser(username);
        _users.Add(user);
        
        // 保存用户列表
        await SaveUsersAsync();
        
        return user;
    }
    
    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task RemoveUserAsync(UserInfo user)
    {
        if (_users.Count <= 1)
        {
            throw new InvalidOperationException("至少需要保留一个用户。");
        }
        
        _users.Remove(user);
        
        // 如果删除的是当前用户，则切换到列表中的第一个用户
        if (CurrentUser == user)
        {
            CurrentUser = _users.FirstOrDefault();
        }
        
        // 保存用户列表
        await SaveUsersAsync();
    }
    
    /// <summary>
    /// 切换当前用户
    /// </summary>
    public void SwitchUser(UserInfo user)
    {
        if (_users.Contains(user))
        {
            CurrentUser = user;
        }
    }
    
    /// <summary>
    /// 获取所有用户
    /// </summary>
    public async Task<List<UserInfo>> GetUsersAsync()
    {
        await LoadUsersAsync();
        return _users.ToList();
    }
    
    /// <summary>
    /// 添加离线用户
    /// </summary>
    public async Task<UserInfo> AddOfflineUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("用户名不能为空");
        }
        
        // 检查用户名是否已存在
        if (_users.Any(u => u.Type == UserType.Offline && u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException($"用户名 {username} 已存在");
        }
        
        // 创建离线用户
        var user = UserInfo.CreateOfflineUser(username);
        _users.Add(user);
        await SaveUsersAsync();
        
        return user;
    }
    
    /// <summary>
    /// 添加微软用户
    /// </summary>
    public async Task<UserInfo> AddMicrosoftUserAsync(string authCode)
    {
        if (string.IsNullOrWhiteSpace(authCode))
        {
            throw new ArgumentException("授权码不能为空");
        }

        // #TODO 实现微软账户登录逻辑
        // 1. 使用授权码获取访问令牌 
        // 2. 使用访问令牌获取Xbox Live令牌
        // 3. 使用Xbox Live令牌获取XSTS令牌
        // 4. 使用XSTS令牌获取Minecraft令牌
        // finished

        // 5. 使用Minecraft令牌获取用户信息 not finished

        var account = await StartMicrosoftAuthAsync();

        // 这里是简化实现，实际需要与Minecraft API交互
        var user = new UserInfo
        {
            Account = account,
            AuthExpireTime = account.OAuthToken.ExpiresAt,
            AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png" // #TODO set skin
        };

        _users.Add(user);
        await SaveUsersAsync();
        
        return user;
    }
    
    /// <summary>
    /// 添加Authlib外置登录
    /// </summary>
    public async Task<UserInfo> AddAuthlibUserAsync(string username, string password, string serverUrl)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(serverUrl))
        {
            throw new ArgumentException("用户名、密码或服务器URL不能为空");
        }

        // TODO: 实现Authlib外置登录逻辑
        // 这里是简化实现，实际需要与外置登录服务器交互

        var account = new YggdrasilAccount
        {
            Capes = [],
            Skins = [],
            ClientToken = string.Empty,
            McAccessToken = string.Empty,
            UserName = username,
            UserProperties = string.Empty,
            Uuid = Guid.NewGuid().ToString()
        };

        var user = new UserInfo
        {
            Account = account,
            ServerUrl = serverUrl,
            AuthExpireTime = DateTime.Now.AddDays(1),
            AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png"
        };
        
        _users.Add(user);
        await SaveUsersAsync();
        
        return user;
    }
    
    /// <summary>
    /// 删除用户
    /// </summary>
    public async Task DeleteUserAsync(string userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            _users.Remove(user);
            
            // 如果删除的是当前用户，重新选择一个用户
            if (CurrentUser?.Id == userId)
            {
                CurrentUser = _users.FirstOrDefault();
                if (CurrentUser != null)
                {
                    CurrentUser.Selected = true;
                }
            }
            
            await SaveUsersAsync();
        }
    }
    
    /// <summary>
    /// 选择用户
    /// </summary>
    public async Task SelectUserAsync(string userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            foreach (var u in _users)
            {
                u.Selected = false;
            }
            
            user.Selected = true;
            user.LastUsed = DateTime.Now;
            CurrentUser = user;
            
            await SaveUsersAsync();
        }
    }
    
    /// <summary>
    /// 修改离线用户名
    /// </summary>
    public async Task RenameOfflineUserAsync(string userId, string newUsername)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId && u.Type == UserType.Offline);
        if (user != null)
        {
            // 检查新用户名是否已存在
            if (_users.Any(u => u.Type == UserType.Offline && u.Id != userId && u.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"用户名 {newUsername} 已存在");
            }

            var newAccount = user.Account as OfflineAccount;
            newAccount.UserName = newUsername;
            newAccount.Uuid = UuidUtils.GenerateOfflineUuid(newUsername);

            await SaveUsersAsync();
        }
        else
        {
            throw new ArgumentException("找不到指定的离线用户");
        }
    }
    
    /// <summary>
    /// 刷新微软用户令牌
    /// </summary>
    public async Task RefreshMicrosoftTokenAsync(string userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId && u.Type == UserType.Microsoft);

        ArgumentNullException.ThrowIfNull(user);

        if (user.Account is MsaAccount account && !string.IsNullOrEmpty(account.OAuthToken.RefreshToken))
        {
            // TODO: 实现令牌刷新逻辑
            // 使用RefreshToken获取新的AccessToken

            // 这里是简化实现

            await SaveUsersAsync();
        }
        else
        {
            throw new ArgumentException("找不到指定的微软用户或刷新令牌为空");
        }
    }

    /// <summary>
    /// 启动Microsoft登录流程
    /// </summary>
    public async Task<MsaAccount> StartMicrosoftAuthAsync()
    {
        var tcs = new TaskCompletionSource<MsaAccount>();
        var observable = _microsoftAuthService.StartDeviceCodeFlow();
        IDisposable? subscription = observable.Subscribe(state =>
        {
            switch (state)
            {
                case DeviceFlowAwaitUser awaitUser:
                    // 可在UI层提示用户输入验证码
                    // #TODO : 提示用户输入验证码
                    break;
                case DeviceFlowSucceeded succeeded:
                    // 这里获得了 MsaAccount
                    var account = succeeded.Account;
                    // 适配为 UserInfo 或直接存储为 BaseAccount
                    tcs.SetResult(account);

                    break;
                case DeviceFlowError error:
                    tcs.SetException(error.Exc ?? new Exception("微软登录失败"));
                    break;
            }
        });

        subscription.Dispose();

        return await tcs.Task;
    }
    
    /// <summary>
    /// 获取用户头像URL
    /// </summary>
    public async Task<string> GetUserAvatarUrlAsync(string userId)
    {
        var user = _users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return "avares://PCL.Neo/Assets/DefaultSkin.png";
        }

        // 如果已经有缓存的头像URL，直接返回
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            return user.AvatarUrl;
        }

        // 否则根据用户类型获取头像
        switch (user.Type)
        {
            case UserType.Microsoft:
                // 从Minecraft皮肤服务获取头像
                try
                {
                    var url = $"https://sessionserver.mojang.com/session/minecraft/profile/{user.UUID}";
                    // var response = await _httpClient.GetStringAsync(url);
                    // 解析响应，获取皮肤URL
                    // 这里简化处理
                }
                catch
                {
                    // 出错时使用默认头像
                }

                break;

            case UserType.Authlib:
                // 从外置登录服务获取头像
                // 略
                break;
            case UserType.Offline:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // 如果没有获取到头像，使用默认头像
        if (string.IsNullOrEmpty(user.AvatarUrl))
        {
            user.AvatarUrl = "avares://PCL.Neo/Assets/DefaultSkin.png";
            await SaveUsersAsync();
        }

        return user.AvatarUrl;
    }
} 