using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Models.Minecraft.Game.Data;
using PCL.Neo.Services;
using PCL.Neo.Views.Home;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCL.Neo.ViewModels.Home;

public class EnvironmentVariable
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ModInfo
{
    public bool IsEnabled { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}

public class VersionComponent
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsCompatible { get; set; } = true;
    public bool IsClickable { get; set; } = false;
}

[SubViewModelOf(typeof(HomeViewModel))]
public partial class GameSettingsViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly StorageService _storageService;
    
    // 版本标题
    [ObservableProperty] private string _versionTitle = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";
    
    // 导航相关
    [ObservableProperty] private int _selectedMenuIndex = 0;
    [ObservableProperty] private object? _currentContentView;
    [ObservableProperty] private object? _currentView;
    
    #region 基本信息
    [ObservableProperty] private string _versionId = string.Empty;
    [ObservableProperty] private string _gameVersionName = string.Empty;
    [ObservableProperty] private string _versionType = string.Empty;
    [ObservableProperty] private string _releaseTime = string.Empty;
    [ObservableProperty] private string _mainClass = string.Empty;
    [ObservableProperty] private string _inheritsFrom = string.Empty;
    #endregion
    
    #region 修改页面
    [ObservableProperty] private ObservableCollection<VersionComponent> _components = new();
    [ObservableProperty] private bool _hasFabricWarning = true;
    [ObservableProperty] private string _minecraftVersion = "1.20.2";
    #endregion
    
    #region 概览页面
    [ObservableProperty] private string _packageName = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";
    [ObservableProperty] private string _packageDescription = "1.20.2, Fabric 0.15.7";
    [ObservableProperty] private string _customIcon = "自动";
    [ObservableProperty] private string _customCategory = "自动";
    #endregion
    
    #region 设置页面
    [ObservableProperty] private string _launchMode = "开启";
    [ObservableProperty] private string _gameWindowTitle = "跟随全局设置";
    [ObservableProperty] private string _customGameInfo = "跟随全局设置";
    [ObservableProperty] private string _gameJava = "跟随全局设置";
    [ObservableProperty] private bool _useGlobalMemory = true;
    [ObservableProperty] private bool _useCustomMemory = false;
    [ObservableProperty] private bool _isCustomMemory = false;
    [ObservableProperty] private double _memorySliderValue = 50;
    [ObservableProperty] private double _usedMemory = 8.3;
    [ObservableProperty] private double _totalMemory = 15.9;
    [ObservableProperty] private double _gameMemory = 4.5;
    [ObservableProperty] private string _serverLoginMethod = "正版登录或离线登录";
    [ObservableProperty] private string _serverAutoJoin = string.Empty;
    [ObservableProperty] private string _memoryOptimization = "跟随全局设置";
    [ObservableProperty] private string _advancedLaunchOptions = string.Empty;
    #endregion
    
    #region 导出页面
    [ObservableProperty] private string _packageVersion = "1.0.0";
    [ObservableProperty] private bool _exportGameCore = true;
    [ObservableProperty] private bool _exportGameSettings = true;
    [ObservableProperty] private bool _exportGameUserInfo = false;
    [ObservableProperty] private bool _exportMods = true;
    [ObservableProperty] private bool _exportModsSettings = true;
    [ObservableProperty] private bool _exportResourcePacks = true;
    [ObservableProperty] private string _selectedResourcePack = "Xray_Ultimate_1.20.2_v5.0.0.zip";
    [ObservableProperty] private bool _exportMultiServerList = false;
    [ObservableProperty] private bool _exportLauncherProgram = false;
    [ObservableProperty] private bool _exportSourceFiles = false;
    [ObservableProperty] private bool _useModrinthUpload = false;
    #endregion
    
    #region Java设置
    [ObservableProperty] private string _javaPath = string.Empty;
    [ObservableProperty] private int _memoryAllocation = 2048;
    [ObservableProperty] private int _maxMemoryMB = 8192;
    [ObservableProperty] private string _memoryAllocationDisplay = "2048 MB";
    [ObservableProperty] private string _jvmArguments = string.Empty;
    #endregion
    
    #region 游戏设置
    [ObservableProperty] private string _gameDirectory = string.Empty;
    [ObservableProperty] private bool _isFullScreen = false;
    [ObservableProperty] private int _gameWidth = 854;
    [ObservableProperty] private int _gameHeight = 480;
    [ObservableProperty] private string _gameArguments = string.Empty;
    [ObservableProperty] private bool _closeAfterLaunch = false;
    [ObservableProperty] private bool _disableAnimation = false;
    [ObservableProperty] private bool _useLegacyLauncher = false;
    #endregion
    
    #region Mods管理
    [ObservableProperty] private ObservableCollection<ModInfo> _mods = new();
    #endregion
    
    #region 高级设置
    [ObservableProperty] private ObservableCollection<string> _downloadSources = new();
    [ObservableProperty] private string _selectedDownloadSource = string.Empty;
    [ObservableProperty] private string _proxyAddress = string.Empty;
    [ObservableProperty] private bool _isProxyEnabled = false;
    [ObservableProperty] private int _maxThreads = 16;
    [ObservableProperty] private ObservableCollection<EnvironmentVariable> _environmentVariables = new();
    [ObservableProperty] private bool _isDebugMode = false;
    [ObservableProperty] private bool _saveGameLog = true;
    [ObservableProperty] private bool _enableCrashAnalysis = true;
    #endregion
    
    #region 性能设置
    [ObservableProperty] private int _renderDistance = 12;
    [ObservableProperty] private ObservableCollection<string> _particleOptions = new();
    [ObservableProperty] private string _selectedParticleOption = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _graphicsOptions = new();
    [ObservableProperty] private string _selectedGraphicsOption = string.Empty;
    [ObservableProperty] private int _maxFrameRate = 60;
    [ObservableProperty] private int _masterVolume = 100;
    [ObservableProperty] private int _musicVolume = 100;
    [ObservableProperty] private int _soundVolume = 100;
    #endregion
    
    public GameSettingsViewModel(INavigationService navigationService, GameService gameService, StorageService storageService)
    {
        _navigationService = navigationService;
        _gameService = gameService;
        _storageService = storageService;
        
        // 初始化组件列表
        Components = new ObservableCollection<VersionComponent>
        {
            new VersionComponent { Name = "Minecraft", Version = "1.20.2", IsClickable = true },
            new VersionComponent { Name = "Forge", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "NeoForge", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "Fabric", Version = "0.15.7", IsClickable = true },
            new VersionComponent { Name = "Fabric API", Version = "点击选择", IsClickable = true },
            new VersionComponent { Name = "Quilt", Version = "与Fabric不兼容", IsCompatible = false },
            new VersionComponent { Name = "OptiFine", Version = "点击选择", IsClickable = true }
        };
        
        // 初始化下拉选项
        DownloadSources = new ObservableCollection<string>
        {
            "官方源",
            "BMCLAPI",
            "MCBBS",
            "自定义源"
        };
        SelectedDownloadSource = "BMCLAPI";
        
        ParticleOptions = new ObservableCollection<string>
        {
            "全部",
            "减少",
            "最小",
            "关闭"
        };
        SelectedParticleOption = "全部";
        
        GraphicsOptions = new ObservableCollection<string>
        {
            "流畅",
            "均衡",
            "高品质",
            "自定义"
        };
        SelectedGraphicsOption = "高品质";
        
        // 初始化环境变量列表
        EnvironmentVariables = new ObservableCollection<EnvironmentVariable>
        {
            new EnvironmentVariable { IsEnabled = true, Name = "JAVA_TOOL_OPTIONS", Value = "-Dfile.encoding=UTF-8" },
            new EnvironmentVariable { IsEnabled = false, Name = "JAVA_OPTS", Value = "-XX:+UseG1GC" }
        };
        
        // 监听菜单索引变化
        this.PropertyChanged += (sender, args) => {
            if (args.PropertyName == nameof(SelectedMenuIndex))
            {
                UpdateContentView();
            }
        };
        
        // 初始化默认视图
        UpdateContentView();
    }
    
    private void UpdateContentView()
    {
        // 根据选中的菜单索引更新内容视图
        switch (SelectedMenuIndex)
        {
            case 0: // 概览
                CurrentContentView = new OverviewView { DataContext = this };
                break;
            case 1: // 设置
                CurrentContentView = new SettingsView { DataContext = this };
                break;
            case 2: // 修改
                CurrentContentView = new ModifyView { DataContext = this };
                break;
            case 3: // 导出
                CurrentContentView = new ExportView { DataContext = this };
                break;
            default:
                CurrentContentView = new SavesView { DataContext = this };
                break;
        }
    }
    
    public async Task Initialize(string versionId)
    {
        VersionId = versionId;
        VersionTitle = versionId;
        
        try
        {
            // 加载版本信息
            var versionInfo = await _gameService.GetVersionInfo(versionId);
            if (versionInfo != null)
            {
                GameVersionName = versionInfo.Name;
                VersionTitle = versionInfo.Name;
                PackageName = versionInfo.Name;
                VersionType = versionInfo.Type;
                ReleaseTime = versionInfo.ReleaseTime;
                MainClass = versionInfo.MainClass;
                InheritsFrom = versionInfo.InheritsFrom ?? "无";
                
                // 加载版本特定设置
                LoadVersionSettings(versionId);
            }
            
            // 加载系统信息
            MaxMemoryMB = _gameService.GetSystemMaxMemoryMB();
            
            // 确保内存分配合理
            if (MemoryAllocation > MaxMemoryMB)
            {
                MemoryAllocation = Math.Min(MaxMemoryMB, 4096);
            }
            
            // 加载模组列表
            await LoadMods(versionId);
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"初始化游戏设置失败: {ex.Message}");
        }
    }
    
    private void LoadVersionSettings(string versionId)
    {
        // 这里应该从配置文件加载特定版本的设置
        // 以下是示例数据
        JavaPath = _gameService.DefaultJavaPath;
        GameDirectory = _gameService.DefaultGameDirectory;
        JvmArguments = "-XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200";
        GameArguments = "";
    }
    
    private async Task LoadMods(string versionId)
    {
        // 这里应该从mods文件夹加载mod列表
        // 以下是示例数据
        Mods = new ObservableCollection<ModInfo>
        {
            new ModInfo { Name = "OptiFine", Version = "1.20.1_HD_U_I5", Author = "sp614x", Description = "优化模组" },
            new ModInfo { Name = "JourneyMap", Version = "5.9.16", Author = "techbrew", Description = "小地图模组" },
            new ModInfo { Name = "Fabric API", Version = "0.92.0", Author = "FabricMC", Description = "Fabric模组加载器API" }
        };
        
        // 实际实现时需要扫描mods文件夹
        await Task.CompletedTask;
    }
    
    partial void OnMemoryAllocationChanged(int value)
    {
        MemoryAllocationDisplay = $"{value} MB";
    }
    
    [RelayCommand]
    private async Task SelectJava()
    {
        var javaPath = await _gameService.SelectJavaPathAsync();
        if (!string.IsNullOrEmpty(javaPath))
        {
            JavaPath = javaPath;
        }
    }
    
    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        var gameDir = await _gameService.SelectGameDirectoryAsync();
        if (!string.IsNullOrEmpty(gameDir))
        {
            GameDirectory = gameDir;
        }
    }
    
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // 保存设置
            // TODO: 实现设置保存逻辑
            
            // 返回上一个页面
            await _navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            // 处理异常
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task Return()
    {
        // 返回上一页
        await _navigationService.GoBackAsync();
    }
    
    [RelayCommand]
    private void EditVersionName()
    {
        // 实现编辑版本名称的逻辑
    }
    
    [RelayCommand]
    private void EditVersionDescription()
    {
        // 实现编辑版本描述的逻辑
    }
    
    [RelayCommand]
    private void AddToCollection()
    {
        // 实现添加到收藏的逻辑
    }
    
    [RelayCommand]
    private void OpenVersionFolder()
    {
        // 实现打开版本文件夹的逻辑
    }
    
    [RelayCommand]
    private void OpenSaveFolder()
    {
        // 实现打开存档文件夹的逻辑
    }
    
    [RelayCommand]
    private void OpenModFolder()
    {
        // 实现打开Mod文件夹的逻辑
    }
    
    [RelayCommand]
    private void ExportStarter()
    {
        // 实现导出启动脚本的逻辑
    }
    
    [RelayCommand]
    private void TestGame()
    {
        // 实现测试游戏的逻辑
    }
    
    [RelayCommand]
    private void CompleteFiles()
    {
        // 实现补全文件的逻辑
    }
    
    [RelayCommand]
    private void Reinstall()
    {
        // 实现重装的逻辑
    }
    
    [RelayCommand]
    private void DeleteVersion()
    {
        // 实现删除版本的逻辑
    }
    
    [RelayCommand]
    private void StartModify()
    {
        // 实现开始修改的逻辑
    }
    
    [RelayCommand]
    private void ReadConfig()
    {
        // 实现读取配置的逻辑
    }
    
    [RelayCommand]
    private void SaveConfig()
    {
        // 实现保存配置的逻辑
    }
    
    [RelayCommand]
    private void ExportPackageGuide()
    {
        // 实现整合包制作指南的逻辑
    }
    
    [RelayCommand]
    private void Export()
    {
        // 实现导出的逻辑
    }
    
    [RelayCommand]
    private void OpenSaveFile()
    {
        // 实现打开存档文件的逻辑
    }
    
    [RelayCommand]
    private void PasteSaveFile()
    {
        // 实现粘贴存档文件的逻辑
    }
} 