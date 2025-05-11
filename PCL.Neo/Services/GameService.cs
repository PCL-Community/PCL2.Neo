using Avalonia.Platform.Storage;
using PCL.Neo.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Models.Minecraft.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public class GameService
{
    private readonly StorageService _storageService;
    private readonly IJavaManager _javaManager;
    public string DefaultGameDirectory { get; }
    public string DefaultJavaPath { get; }

    public GameService(StorageService storageService, IJavaManager javaManager)
    {
        _storageService = storageService;
        _javaManager = javaManager;

        // 设置默认的Minecraft目录
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        DefaultGameDirectory = Path.Combine(userProfile, ".minecraft");

        // 设置默认的Java路径，使用JavaLocator
        DefaultJavaPath = JavaLocator.GetDefaultJavaPath();

        // 初始化Java列表
        _javaManager.JavaListInit();
    }

    /// <summary>
    /// 查找系统中的默认Java路径
    /// </summary>
    public async Task<string?> FindDefaultJavaPathAsync()
    {
        await _javaManager.JavaListInit();

        if (_javaManager.JavaList.Count > 0)
        {
            // 使用DirectoryPath属性获取Java可执行文件路径
            var javaRuntime = _javaManager.JavaList[0];
            string javaExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javaw.exe" : "java";
            return Path.Combine(javaRuntime.DirectoryPath, "bin", javaExecutable);
        }

        return JavaLocator.GetDefaultJavaPath();
    }

    /// <summary>
    /// 获取系统最大可用内存 (MB)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public int GetSystemMaxMemoryMB()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                NativeMemoryStatus memoryStatus = new NativeMemoryStatus();
                memoryStatus.dwLength = (uint)Marshal.SizeOf(typeof(NativeMemoryStatus));

                if (GlobalMemoryStatusEx(ref memoryStatus))
                {
                    return (int)(memoryStatus.ullTotalPhys / 1024 / 1024);
                }
            }
            else
            {
                // 通过JavaManager获取系统信息
                return 8192; // 8GB 作为默认值，后续可改进
            }
        }
        catch
        {
            // 出错时使用默认值
        }

        return 4096; // 默认4GB
    }

    /// <summary>
    /// 获取游戏版本列表
    /// </summary>
    public async Task<List<VersionInfo>> GetVersionsAsync(string? minecraftDirectory = null, bool forceRefresh = false)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;

        // 获取本地版本
        var localVersions = await Versions.GetLocalVersionsAsync(directory);

        // 如果需要强制刷新或者本地版本为空，则获取远程版本
        if (forceRefresh || localVersions.Count == 0)
        {
            try
            {
                var remoteVersions = await Versions.GetRemoteVersionsAsync();

                // 合并版本列表，保留本地版本的信息
                Dictionary<string, VersionInfo> versionDict = new();

                foreach (var version in localVersions)
                {
                    versionDict[version.Id] = version;
                }

                foreach (var version in remoteVersions)
                {
                    if (!versionDict.ContainsKey(version.Id))
                    {
                        versionDict[version.Id] = version;
                    }
                }

                return new List<VersionInfo>(versionDict.Values);
            }
            catch
            {
                // 如果获取远程版本失败，则返回本地版本
                return localVersions;
            }
        }

        return localVersions;
    }

    /// <summary>
    /// 选择游戏目录
    /// </summary>
    public async Task<string> SelectGameDirectoryAsync()
    {
        var folder = await _storageService.PickFolderAsync();
        return folder ?? DefaultGameDirectory;
    }

    /// <summary>
    /// 选择Java路径
    /// </summary>
    public async Task<string> SelectJavaPathAsync()
    {
        var filters = new List<FilePickerFileType>
        {
            new("Java程序")
            {
                Patterns = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new[] { "java.exe", "javaw.exe" }
                    : new[] { "java" }
            },
            new("所有文件")
            {
                Patterns = new[] { "*" }
            }
        };

        var file = await _storageService.PickFileAsync(filters);
        return file ?? DefaultJavaPath;
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    public async Task LaunchGameAsync(Models.Minecraft.Game.LaunchOptions options)
    {
        // 验证参数
        if (string.IsNullOrEmpty(options.VersionId))
        {
            throw new ArgumentException("版本ID不能为空");
        }

        if (string.IsNullOrEmpty(options.JavaPath) || !File.Exists(options.JavaPath))
        {
            throw new ArgumentException("Java路径无效");
        }

        if (string.IsNullOrEmpty(options.MinecraftDirectory) || !Directory.Exists(options.MinecraftDirectory))
        {
            throw new ArgumentException("游戏目录无效");
        }

        // 获取版本信息
        var versionInfo = await Versions.GetVersionByIdAsync(options.MinecraftDirectory, options.VersionId);
        if (versionInfo == null)
        {
            throw new Exception($"找不到版本: {options.VersionId}");
        }

        // 获取或创建版本继承的父版本
        if (!string.IsNullOrEmpty(versionInfo.InheritsFrom))
        {
            var parentVersion = await Versions.GetVersionByIdAsync(options.MinecraftDirectory, versionInfo.InheritsFrom);
            if (parentVersion == null)
            {
                throw new Exception($"找不到父版本: {versionInfo.InheritsFrom}");
            }
        }

        // 构建启动命令行
        string javaArgs = $"-Xmx{options.MaxMemoryMB}M -Xms{options.MinMemoryMB}M";
        string mainClass = versionInfo.MainClass;

        // 添加游戏参数
        string gameArgs = await BuildGameArgumentsAsync(versionInfo, options);

        // 启动游戏进程
        Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = options.JavaPath,
                Arguments = $"{javaArgs} {mainClass} {gameArgs}",
                WorkingDirectory = options.GameDirectory ?? options.MinecraftDirectory,
                UseShellExecute = true
            }
        };

        process.Start();

        // 等待进程启动
        await Task.Delay(1000);

        // 如果设置为启动后关闭启动器，则执行相应操作
        if (options.CloseAfterLaunch)
        {
            // TODO: 实现关闭启动器的逻辑
        }
    }

    /// <summary>
    /// 构建游戏启动参数
    /// </summary>
    private async Task<string> BuildGameArgumentsAsync(VersionInfo versionInfo, Models.Minecraft.Game.LaunchOptions options)
    {
        // 构建参数字符串
        var gameDir = options.GameDirectory ?? options.MinecraftDirectory;
        var assetsDir = Path.Combine(options.MinecraftDirectory, "assets");
        var assetIndex = versionInfo.AssetIndex?.Id ?? "legacy";

        // 使用版本的游戏参数或Minecraft参数
        if (versionInfo.Arguments?.Game != null)
        {
            // 新版本格式
            var args = new System.Text.StringBuilder();
            foreach (var arg in versionInfo.Arguments.Game)
            {
                if (arg is string strArg)
                {
                    var formattedArg = strArg
                        .Replace("${auth_player_name}", options.Username)
                        .Replace("${version_name}", versionInfo.Id)
                        .Replace("${game_directory}", $"\"{gameDir}\"")
                        .Replace("${assets_root}", $"\"{assetsDir}\"")
                        .Replace("${assets_index_name}", assetIndex)
                        .Replace("${auth_uuid}", options.UUID)
                        .Replace("${auth_access_token}", options.AccessToken)
                        .Replace("${user_type}", options.UserType)
                        .Replace("${version_type}", versionInfo.Type);

                    args.Append($"{formattedArg} ");
                }
            }

            return args.ToString();
        }
        else if (!string.IsNullOrEmpty(versionInfo.MinecraftArguments))
        {
            // 旧版本格式
            return versionInfo.MinecraftArguments
                .Replace("${auth_player_name}", options.Username)
                .Replace("${version_name}", versionInfo.Id)
                .Replace("${game_directory}", $"\"{gameDir}\"")
                .Replace("${assets_root}", $"\"{assetsDir}\"")
                .Replace("${assets_index_name}", assetIndex)
                .Replace("${auth_uuid}", options.UUID)
                .Replace("${auth_access_token}", options.AccessToken)
                .Replace("${user_type}", options.UserType)
                .Replace("${version_type}", versionInfo.Type);
        }

        // 如果两种方式都不可用，使用基本参数
        return $"--username {options.Username} --gameDir \"{gameDir}\" --assetsDir \"{assetsDir}\" --assetIndex {assetIndex}";
    }

    /// <summary>
    /// 扫描特定目录中的游戏版本
    /// </summary>
    public async Task<List<VersionInfo>> ScanDirectoryForVersionsAsync(string directory)
    {
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            throw new ArgumentException("目录无效");
        }

        // 检查是否包含versions子目录
        string versionsDir = Path.Combine(directory, "versions");
        if (!Directory.Exists(versionsDir))
        {
            // 如果没有versions子目录，检查当前目录是否直接包含版本文件
            if (Directory.GetFiles(directory, "*.json").Length > 0)
            {
                // 创建versions目录和版本子目录
                Directory.CreateDirectory(versionsDir);

                // 处理直接包含版本文件的情况
                foreach (var jsonFile in Directory.GetFiles(directory, "*.json"))
                {
                    try
                    {
                        // 读取json文件内容
                        string jsonContent = await File.ReadAllTextAsync(jsonFile);
                        var versionData = System.Text.Json.JsonDocument.Parse(jsonContent);

                        // 尝试获取版本ID
                        if (versionData.RootElement.TryGetProperty("id", out var idElement))
                        {
                            string versionId = idElement.GetString() ?? Path.GetFileNameWithoutExtension(jsonFile);

                            // 创建版本目录
                            string versionDir = Path.Combine(versionsDir, versionId);
                            Directory.CreateDirectory(versionDir);

                            // 复制json文件
                            string targetJsonFile = Path.Combine(versionDir, $"{versionId}.json");
                            File.Copy(jsonFile, targetJsonFile, true);
                        }
                    }
                    catch
                    {
                        // 忽略处理单个文件时的错误
                        continue;
                    }
                }
            }

            // 再次检查是否有有效版本
            if (!Directory.Exists(versionsDir) || Directory.GetDirectories(versionsDir).Length == 0)
            {
                return new List<VersionInfo>();
            }
        }

        // 扫描versions子目录中的版本
        return await Versions.GetLocalVersionsAsync(directory);
    }

    /// <summary>
    /// 打开游戏设置页面
    /// </summary>
    public void OpenGameSettings(string versionId)
    {
        // 已在UI中实现，无需在Service层处理
    }

    /// <summary>
    /// 获取指定版本ID的详细信息
    /// </summary>
    public async Task<VersionInfo?> GetVersionInfo(string versionId, string? minecraftDirectory = null)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        return await Versions.GetVersionByIdAsync(directory, versionId);
    }

    /// <summary>
    /// 刷新Java安装列表
    /// </summary>
    public async Task RefreshJavaInstallationsAsync()
    {
        await _javaManager.Refresh();
    }

    /// <summary>
    /// 添加自定义Java路径
    /// </summary>
    public async Task AddCustomJavaAsync(string javaPath)
    {
        string javaDir = Path.GetDirectoryName(javaPath) ?? string.Empty;
        if (!string.IsNullOrEmpty(javaDir))
        {
            await _javaManager.ManualAdd(javaDir);
        }
    }

    /// <summary>
    /// 获取所有已安装的Java列表
    /// </summary>
    public List<JavaRuntime> GetJavaInstallations()
    {
        return _javaManager.JavaList;
    }

    /// <summary>
    /// 下载Minecraft版本
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="progressCallback">进度回调</param>
    /// <returns>下载是否成功</returns>
    public async Task<bool> DownloadVersionAsync(string versionId, IProgress<int>? progressCallback = null)
    {
        try
        {
            // 获取远程版本列表
            var remoteVersions = await Versions.GetRemoteVersionsAsync();
            var version = remoteVersions.FirstOrDefault(v => v.Id == versionId);

            if (version == null || version.Downloads?.Client?.Url == null)
            {
                return false;
            }

            // 创建版本目录
            string versionDirectory = Path.Combine(DefaultGameDirectory, "versions", versionId);
            Directory.CreateDirectory(versionDirectory);

            // 下载版本JSON
            string jsonUrl = version.Downloads.Client.Url;
            var versionInfo = await Versions.DownloadVersionInfoAsync(versionId, jsonUrl);

            if (versionInfo == null)
            {
                return false;
            }

            // 保存JSON文件
            string jsonPath = Path.Combine(versionDirectory, $"{versionId}.json");
            await File.WriteAllTextAsync(jsonPath, System.Text.Json.JsonSerializer.Serialize(versionInfo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            }));

            // 下载客户端JAR
            string clientJarUrl = versionInfo.Downloads?.Client?.Url ?? "";
            if (string.IsNullOrEmpty(clientJarUrl))
            {
                return false;
            }

            string jarPath = Path.Combine(versionDirectory, $"{versionId}.jar");
            await DownloadFileAsync(clientJarUrl, jarPath, progressCallback);

            // 下载所需资源和库文件
            await DownloadAssetsAsync(versionInfo, progressCallback);
            await DownloadLibrariesAsync(versionInfo, progressCallback);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载版本时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 下载Minecraft资源文件
    /// </summary>
    private async Task DownloadAssetsAsync(VersionInfo versionInfo, IProgress<int>? progressCallback = null)
    {
        if (versionInfo.AssetIndex == null || string.IsNullOrEmpty(versionInfo.AssetIndex.Url))
        {
            return;
        }

        string assetsDir = Path.Combine(DefaultGameDirectory, "assets");
        string indexesDir = Path.Combine(assetsDir, "indexes");
        string objectsDir = Path.Combine(assetsDir, "objects");

        Directory.CreateDirectory(indexesDir);
        Directory.CreateDirectory(objectsDir);

        // 下载资源索引
        string indexId = versionInfo.AssetIndex.Id;
        string indexPath = Path.Combine(indexesDir, $"{indexId}.json");

        using var httpClient = new HttpClient();
        string indexJson = await httpClient.GetStringAsync(versionInfo.AssetIndex.Url);
        await File.WriteAllTextAsync(indexPath, indexJson);

        // 解析资源索引
        var assetIndex = System.Text.Json.JsonDocument.Parse(indexJson);
        if (assetIndex.RootElement.TryGetProperty("objects", out var objectsElement))
        {
            int totalAssets = objectsElement.EnumerateObject().Count();
            int currentAsset = 0;

            foreach (var asset in objectsElement.EnumerateObject())
            {
                if (asset.Value.TryGetProperty("hash", out var hashElement))
                {
                    string hash = hashElement.GetString() ?? "";
                    if (!string.IsNullOrEmpty(hash))
                    {
                        string prefix = hash.Substring(0, 2);
                        string objectPath = Path.Combine(objectsDir, prefix, hash);
                        string objectDir = Path.GetDirectoryName(objectPath) ?? "";

                        if (!Directory.Exists(objectDir))
                        {
                            Directory.CreateDirectory(objectDir);
                        }

                        if (!File.Exists(objectPath))
                        {
                            string url = $"https://resources.download.minecraft.net/{prefix}/{hash}";
                            try
                            {
                                await DownloadFileAsync(url, objectPath);
                            }
                            catch
                            {
                                // 忽略单个资源下载失败
                            }
                        }

                        currentAsset++;
                        progressCallback?.Report((int)(currentAsset * 100.0 / totalAssets));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 下载Minecraft库文件
    /// </summary>
    private async Task DownloadLibrariesAsync(VersionInfo versionInfo, IProgress<int>? progressCallback = null)
    {
        if (versionInfo.Libraries == null || versionInfo.Libraries.Count == 0)
        {
            return;
        }

        string librariesDir = Path.Combine(DefaultGameDirectory, "libraries");
        Directory.CreateDirectory(librariesDir);

        int totalLibraries = versionInfo.Libraries.Count;
        int currentLibrary = 0;

        foreach (var library in versionInfo.Libraries)
        {
            if (library?.Downloads?.Artifact?.Url != null)
            {
                string path = library.Downloads.Artifact.Path;
                string url = library.Downloads.Artifact.Url;

                string libraryPath = Path.Combine(librariesDir, path);
                string libraryDir = Path.GetDirectoryName(libraryPath) ?? "";

                if (!Directory.Exists(libraryDir))
                {
                    Directory.CreateDirectory(libraryDir);
                }

                if (!File.Exists(libraryPath))
                {
                    try
                    {
                        await DownloadFileAsync(url, libraryPath);
                    }
                    catch
                    {
                        // 忽略单个库文件下载失败
                    }
                }
            }

            // 下载本机库文件
            if (library?.Downloads?.Classifiers != null)
            {
                var nativeKey = GetNativeKey();
                if (nativeKey != null && library.Downloads.Classifiers.TryGetValue(nativeKey, out var nativeArtifact))
                {
                    if (nativeArtifact.Url != null && nativeArtifact.Path != null)
                    {
                        string nativePath = Path.Combine(librariesDir, nativeArtifact.Path);
                        string nativeDir = Path.GetDirectoryName(nativePath) ?? "";

                        if (!Directory.Exists(nativeDir))
                        {
                            Directory.CreateDirectory(nativeDir);
                        }

                        if (!File.Exists(nativePath))
                        {
                            try
                            {
                                await DownloadFileAsync(nativeArtifact.Url, nativePath);
                            }
                            catch
                            {
                                // 忽略单个本机库文件下载失败
                            }
                        }
                    }
                }
            }

            currentLibrary++;
            progressCallback?.Report((int)(currentLibrary * 100.0 / totalLibraries));
        }
    }

    /// <summary>
    /// 获取当前系统的本机库键
    /// </summary>
    private string? GetNativeKey()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "natives-windows-64",
                Architecture.X86 => "natives-windows-32",
                Architecture.Arm64 => "natives-windows-arm64",
                _ => "natives-windows"
            };
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "natives-linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "natives-osx";
        }

        return null;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    private async Task DownloadFileAsync(string url, string savePath, IProgress<int>? progress = null)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long bytesRead = 0;
        int count;

        while ((count = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, count));
            bytesRead += count;

            if (totalBytes.HasValue && progress != null)
            {
                int percentage = (int)(bytesRead * 100 / totalBytes.Value);
                progress.Report(percentage);
            }
        }
    }

    /// <summary>
    /// 检查版本是否已安装
    /// </summary>
    public bool IsVersionInstalled(string versionId, string? minecraftDirectory = null)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        string versionDir = Path.Combine(directory, "versions", versionId);
        string jsonPath = Path.Combine(versionDir, $"{versionId}.json");
        string jarPath = Path.Combine(versionDir, $"{versionId}.jar");

        return Directory.Exists(versionDir) && File.Exists(jsonPath) && File.Exists(jarPath);
    }

    /// <summary>
    /// 删除游戏版本
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <param name="minecraftDirectory">游戏目录</param>
    public async Task DeleteVersionAsync(string versionId, string? minecraftDirectory = null)
    {
        string directory = minecraftDirectory ?? DefaultGameDirectory;
        string versionDir = Path.Combine(directory, "versions", versionId);

        if (Directory.Exists(versionDir))
        {
            // 删除版本目录及其所有内容
            try
            {
                // 先删除所有文件
                foreach (var file in Directory.GetFiles(versionDir))
                {
                    File.Delete(file);
                }

                // 再删除所有子目录
                foreach (var dir in Directory.GetDirectories(versionDir))
                {
                    Directory.Delete(dir, true);
                }

                // 最后删除版本目录
                Directory.Delete(versionDir);

                // 延迟一下以确保文件系统操作完成
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                throw new Exception($"删除版本时出错: {ex.Message}");
            }
        }
        else
        {
            throw new DirectoryNotFoundException($"找不到版本目录: {versionDir}");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMemoryStatus
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref NativeMemoryStatus lpBuffer);
}