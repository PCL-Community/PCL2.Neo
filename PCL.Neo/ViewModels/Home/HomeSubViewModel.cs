using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Models.Minecraft.Game;
using PCL.Neo.Models.Minecraft.Game.Data;
using PCL.Neo.Models.Minecraft.Java;
using PCL.Neo.Models.User;
using PCL.Neo.Services;
using PCL.Neo.ViewModels;
using PCL.Neo.ViewModels.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace PCL.Neo.ViewModels.Home;

// 定义主页布局类型
public enum HomeLayoutType
{
    Default,
    News,
    Info,
    Simple
}

[SubViewModelOf(typeof(HomeViewModel))]
public partial class HomeSubViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameLauncher _gameLauncher;
    private readonly UserService _userService;
    private readonly StorageService _storageService;
    private readonly GameService _gameService;
    
    [ObservableProperty]
    private ObservableCollection<GameVersion> _gameVersions = new();
    
    [ObservableProperty]
    private GameVersion? _selectedGameVersion;

    [ObservableProperty]
    private ObservableCollection<UserInfo> _users = new();
    
    [ObservableProperty]
    private UserInfo? _selectedUser;
    
    [ObservableProperty]
    private string _gameDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
    
    [ObservableProperty]
    private string _javaPath = JavaLocator.GetDefaultJavaPath();
    
    [ObservableProperty]
    private int _memoryAllocation = 2048;
    
    [ObservableProperty]
    private int _maxMemoryMB = 8192;

    [ObservableProperty]
    private bool _isLaunching;

    [ObservableProperty]
    private string _statusMessage = "等待启动";
    
    [ObservableProperty]
    private string _memoryAllocationDisplay = "2048 MB";
    
    [ObservableProperty]
    private HomeLayoutType _currentLayout = HomeLayoutType.Default;
    
    [ObservableProperty]
    private string _newsContent = string.Empty;
    
    [ObservableProperty]
    private List<GameNewsItem> _newsItems = new();
    
    [ObservableProperty]
    private List<GameInfoItem> _infoItems = new();
    
    // 首页布局可见性
    [ObservableProperty]
    private bool _isDefaultLayoutVisible = true;
    
    [ObservableProperty]
    private bool _isNewsLayoutVisible = false;
    
    [ObservableProperty]
    private bool _isInfoLayoutVisible = false;
    
    [ObservableProperty]
    private bool _isSimpleLayoutVisible = false;

    public HomeSubViewModel(
        INavigationService navigationService, 
        GameLauncher gameLauncher,
        UserService userService,
        StorageService storageService,
        GameService gameService)
    {
        _navigationService = navigationService;
        _gameLauncher = gameLauncher;
        _userService = userService;
        _storageService = storageService;
        _gameService = gameService;
        
        // 加载游戏版本
        LoadGameVersions();
        
        // 加载用户列表
        InitializeUserList();
        
        // 获取系统内存信息，设置最大可用内存
        DetectSystemMemory();
    }
    
    private async void LoadGameVersions()
    {
        try
        {
            StatusMessage = "正在加载版本列表...";
            var versions = await Versions.GetLocalVersionsAsync(GameDirectory);
            
            GameVersions.Clear();
            foreach (var version in versions)
            {
                var gameVersion = new GameVersion 
                { 
                    Id = version.Id, 
                    Name = version.Name, 
                    Type = version.Type,
                    ReleaseTime = version.ReleaseTime
                };
                GameVersions.Add(gameVersion);
            }
            
            // 按发布时间排序，最新的在前面
            var sortedVersions = GameVersions.OrderByDescending(v => v.ReleaseTime).ToList();
            GameVersions.Clear();
            foreach (var version in sortedVersions)
            {
                GameVersions.Add(version);
            }
            
            if (GameVersions.Count > 0)
            {
                SelectedGameVersion = GameVersions[0];
            }
            
            StatusMessage = $"已加载 {GameVersions.Count} 个版本";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载版本列表失败: {ex.Message}";
        }
    }
    
    private void InitializeUserList()
    {
        // 监听用户列表变化
        Users = new ObservableCollection<UserInfo>(_userService.Users);
        
        // 设置当前选中用户
        SelectedUser = _userService.CurrentUser;
        
        // 监听用户切换
        this.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(SelectedUser) && SelectedUser != null)
            {
                _userService.SwitchUser(SelectedUser);
            }
        };
    }
    
    private void DetectSystemMemory()
    {
        try
        {
            // 获取系统总内存 (以MB为单位)
            long totalMemoryMB = 0;
            
            if (OperatingSystem.IsWindows())
            {
                var memoryStatus = new NativeMemoryStatus();
                if (NativeMethods.GlobalMemoryStatusEx(ref memoryStatus))
                {
                    totalMemoryMB = (long)(memoryStatus.TotalPhys / (1024 * 1024));
                }
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                try
                {
                    // 在Linux/macOS上读取内存信息
                    string output = string.Empty;
                    if (OperatingSystem.IsLinux())
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "free",
                                Arguments = "-m",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        var lines = output.Split('\n');
                        if (lines.Length >= 2)
                        {
                            var memLine = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (memLine.Length >= 2 && long.TryParse(memLine[1], out long mem))
                            {
                                totalMemoryMB = mem;
                            }
                        }
                    }
                    else // macOS
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "sysctl",
                                Arguments = "-n hw.memsize",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        output = process.StandardOutput.ReadToEnd().Trim();
                        process.WaitForExit();
                        
                        if (long.TryParse(output, out long memBytes))
                        {
                            totalMemoryMB = memBytes / (1024 * 1024);
                        }
                    }
                }
                catch
                {
                    // 如果获取失败，使用默认值
                    totalMemoryMB = 8192;
                }
            }
            
            // 设置最大内存为系统可用内存的75% (避免占用过多系统资源)
            if (totalMemoryMB > 0)
            {
                MaxMemoryMB = (int)(totalMemoryMB * 0.75);
                // 默认分配最大内存的1/4，但至少1GB，最多4GB
                MemoryAllocation = Math.Min(4096, Math.Max(1024, MaxMemoryMB / 4));
            }
            else
            {
                // 如果无法获取内存信息，使用默认值
                MaxMemoryMB = 8192;
                MemoryAllocation = 2048;
            }
        }
        catch
        {
            // 出错时使用默认值
            MaxMemoryMB = 8192;
            MemoryAllocation = 2048;
        }
    }

    [RelayCommand]
    private async Task NavigateToDownloadMod()
    {
        await _navigationService.GotoAsync<DownloadModViewModel>();
    }
    
    [RelayCommand]
    private void ManageVersions()
    {
        // TODO: 实现版本管理功能
        StatusMessage = "版本管理功能开发中...";
    }
    
    [RelayCommand]
    private async Task SelectJava()
    {
        try
        {
            var javaPath = await _storageService.SelectFile("选择Java可执行文件");
            
            if (!string.IsNullOrEmpty(javaPath))
            {
                JavaPath = javaPath;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"选择Java失败: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        try
        {
            var folderPath = await _storageService.SelectFolder("选择游戏目录");
            
            if (!string.IsNullOrEmpty(folderPath))
            {
                GameDirectory = folderPath;
                
                // 重新加载版本列表
                LoadGameVersions();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"选择游戏目录失败: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task AddUser()
    {
        try
        {
            // TODO: 实现添加用户的UI
            var username = "NewPlayer";  // 这里应该从UI获取输入
            
            var newUser = await _userService.AddUserAsync(username);
            
            // 更新用户列表
            Users.Clear();
            foreach (var user in _userService.Users)
            {
                Users.Add(user);
            }
            
            // 选择新添加的用户
            SelectedUser = newUser;
        }
        catch (Exception ex)
        {
            StatusMessage = $"添加用户失败: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task LaunchGame()
    {
        if (SelectedGameVersion == null)
        {
            StatusMessage = "请先选择游戏版本";
            return;
        }
        
        if (SelectedUser == null)
        {
            StatusMessage = "请先选择用户";
            return;
        }
        
        try
        {
            IsLaunching = true;
            StatusMessage = "正在启动游戏...";
            
            var launchOptions = new Models.Minecraft.Game.LaunchOptions
            {
                VersionId = SelectedGameVersion.Id,
                JavaPath = JavaPath,
                MinecraftDirectory = GameDirectory,
                MaxMemoryMB = MemoryAllocation,
                Username = SelectedUser.Username,
                UUID = SelectedUser.UUID
            };
            
            await _gameLauncher.LaunchAsync(launchOptions);
            StatusMessage = "游戏已启动";
        }
        catch (Exception ex)
        {
            StatusMessage = $"启动游戏失败: {ex.Message}";
        }
        finally
        {
            IsLaunching = false;
        }
    }
    
    [RelayCommand]
    private void RefreshVersionList()
    {
        LoadGameVersions();
    }
    
    [RelayCommand]
    private async Task ViewGameLogs()
    {
        // 导航到日志查看界面
        await _navigationService.GotoAsync<LogViewModel>();
    }
    
    [RelayCommand]
    private async Task ExportGameLogs()
    {
        try
        {
            var filePath = await _storageService.SaveFile("导出游戏日志", $"PCL.Neo游戏日志_{DateTime.Now:yyyyMMdd_HHmmss}", ".log");
            
            if (!string.IsNullOrEmpty(filePath))
            {
                await _gameLauncher.ExportGameLogsAsync(filePath);
                StatusMessage = "日志导出成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出日志失败: {ex.Message}";
        }
    }

    partial void OnMemoryAllocationChanged(int value)
    {
        MemoryAllocationDisplay = $"{value} MB";
    }
    
    partial void OnCurrentLayoutChanged(HomeLayoutType value)
    {
        IsDefaultLayoutVisible = value == HomeLayoutType.Default;
        IsNewsLayoutVisible = value == HomeLayoutType.News;
        IsInfoLayoutVisible = value == HomeLayoutType.Info;
        IsSimpleLayoutVisible = value == HomeLayoutType.Simple;
    }
}

public class GameVersion
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ReleaseTime { get; set; } = string.Empty;
}

// Windows内存信息结构
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct NativeMemoryStatus
{
    public uint Length;
    public uint MemoryLoad;
    public ulong TotalPhys;
    public ulong AvailPhys;
    public ulong TotalPageFile;
    public ulong AvailPageFile;
    public ulong TotalVirtual;
    public ulong AvailVirtual;
    public ulong AvailExtendedVirtual;
    
    public NativeMemoryStatus()
    {
        Length = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMemoryStatus));
    }
}

// Windows内存API
public static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    public static extern bool GlobalMemoryStatusEx(ref NativeMemoryStatus lpBuffer);
}

// 新闻项的数据模型
public class GameNewsItem
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

// 信息项的数据模型
public class GameInfoItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}