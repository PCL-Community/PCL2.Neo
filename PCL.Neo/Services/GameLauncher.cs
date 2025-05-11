using PCL.Neo.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Game.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public class GameLauncher
{
    private readonly Log _gameLog;

    public GameLauncher()
    {
        _gameLog = new Log();
    }

    /// <summary>
    /// 启动游戏
    /// </summary>
    public async Task<Process> LaunchAsync(Models.Minecraft.Game.LaunchOptions options)
    {
        // 获取版本信息
        var versionInfo = await Versions.GetVersionByIdAsync(options.MinecraftDirectory, options.VersionId);
        if (versionInfo == null)
        {
            throw new Exception($"找不到版本: {options.VersionId}");
        }

        // 如果是继承版本，递归加载父版本
        var fullVersionInfo = await ResolveInheritedVersionAsync(options.MinecraftDirectory, versionInfo);

        // 构建启动命令
        var arguments = await BuildArgumentsAsync(fullVersionInfo, options);

        // 创建并配置进程启动信息
        var processStartInfo = new ProcessStartInfo
        {
            FileName = options.JavaPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false,
            WorkingDirectory = string.IsNullOrEmpty(options.GameDirectory)
                ? options.MinecraftDirectory
                : options.GameDirectory
        };

        // 启动进程
        var process = new Process
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };

        // 订阅日志输出事件
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _gameLog.AddLog(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _gameLog.AddLog(e.Data, isError: true);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    /// <summary>
    /// 解析继承版本
    /// </summary>
    private async Task<VersionInfo> ResolveInheritedVersionAsync(string minecraftDirectory, VersionInfo versionInfo)
    {
        if (string.IsNullOrEmpty(versionInfo.InheritsFrom))
        {
            return versionInfo;
        }

        var parentVersionInfo = await Versions.GetVersionByIdAsync(minecraftDirectory, versionInfo.InheritsFrom);
        if (parentVersionInfo == null)
        {
            throw new Exception($"找不到父版本: {versionInfo.InheritsFrom}");
        }

        // 递归解析父版本
        parentVersionInfo = await ResolveInheritedVersionAsync(minecraftDirectory, parentVersionInfo);

        // 合并父子版本信息
        return MergeVersionInfo(parentVersionInfo, versionInfo);
    }

    /// <summary>
    /// 合并版本信息
    /// </summary>
    private VersionInfo MergeVersionInfo(VersionInfo parent, VersionInfo child)
    {
        // 创建新版本信息，优先使用子版本的内容
        var result = new VersionInfo
        {
            Id = child.Id,
            Name = child.Name,
            Type = child.Type,
            MainClass = !string.IsNullOrEmpty(child.MainClass) ? child.MainClass : parent.MainClass,
            MinecraftArguments = child.MinecraftArguments ?? parent.MinecraftArguments,
            ReleaseTime = child.ReleaseTime,
            Time = child.Time,
            Assets = child.Assets ?? parent.Assets,
        };

        // 处理AssetIndex (解决不明确引用)
        if (child.AssetIndex != null)
        {
            result.AssetIndex = child.AssetIndex;
        }
        else if (parent.AssetIndex != null)
        {
            result.AssetIndex = parent.AssetIndex;
        }

        // 处理Downloads (解决不明确引用)
        if (child.Downloads != null)
        {
            result.Downloads = child.Downloads;
        }
        else if (parent.Downloads != null)
        {
            result.Downloads = parent.Downloads;
        }

        // 处理JavaVersion (解决不明确引用)
        if (child.JavaVersion != null)
        {
            result.JavaVersion = child.JavaVersion;
        }
        else if (parent.JavaVersion != null)
        {
            result.JavaVersion = parent.JavaVersion;
        }

        // 合并Arguments
        if (child.Arguments != null || parent.Arguments != null)
        {
            result.Arguments = new Arguments();

            // 初始化Game和Jvm集合
            if (result.Arguments.Game == null)
                result.Arguments.Game = new List<object>();

            if (result.Arguments.Jvm == null)
                result.Arguments.Jvm = new List<object>();

            // 合并Game参数
            if (parent.Arguments?.Game != null)
            {
                result.Arguments.Game.AddRange(parent.Arguments.Game);
            }
            if (child.Arguments?.Game != null)
            {
                result.Arguments.Game.AddRange(child.Arguments.Game);
            }

            // 合并JVM参数
            if (parent.Arguments?.Jvm != null)
            {
                result.Arguments.Jvm.AddRange(parent.Arguments.Jvm);
            }
            if (child.Arguments?.Jvm != null)
            {
                result.Arguments.Jvm.AddRange(child.Arguments.Jvm);
            }
        }

        // 合并Libraries
        result.Libraries = new List<Library>();
        if (parent.Libraries != null)
        {
            result.Libraries.AddRange(parent.Libraries);
        }
        if (child.Libraries != null)
        {
            result.Libraries.AddRange(child.Libraries);
        }

        return result;
    }

    /// <summary>
    /// 构建启动参数
    /// </summary>
    private async Task<string> BuildArgumentsAsync(VersionInfo versionInfo, Models.Minecraft.Game.LaunchOptions options)
    {
        var args = new StringBuilder();

        // 添加内存参数
        args.Append($"-Xmx{options.MaxMemoryMB}M ");
        args.Append($"-Xms{options.MinMemoryMB}M ");

        // 添加系统参数
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            args.Append("-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump ");
        }

        // 添加游戏目录
        var gameDir = string.IsNullOrEmpty(options.GameDirectory) ? options.MinecraftDirectory : options.GameDirectory;
        args.Append($"-Dminecraft.applet.TargetDirectory=\"{gameDir}\" ");

        // TODO: 添加更多JVM参数

        // 添加主类
        args.Append($"{versionInfo.MainClass} ");

        // 添加游戏参数
        if (versionInfo.MinecraftArguments != null)
        {
            // 旧版本参数格式
            var gameArgs = versionInfo.MinecraftArguments
                .Replace("${auth_player_name}", options.Username)
                .Replace("${version_name}", versionInfo.Id)
                .Replace("${game_directory}", $"\"{gameDir}\"")
                .Replace("${assets_root}", $"\"{Path.Combine(options.MinecraftDirectory, "assets")}\"")
                .Replace("${assets_index_name}", versionInfo.AssetIndex?.Id ?? "legacy")
                .Replace("${auth_uuid}", options.UUID)
                .Replace("${auth_access_token}", options.AccessToken)
                .Replace("${user_type}", "mojang") // TODO: 支持不同的认证类型
                .Replace("${version_type}", versionInfo.Type)
                .Replace("${user_properties}", "{}") // TODO: 支持用户属性
                .Replace("${game_assets}", $"\"{Path.Combine(options.MinecraftDirectory, "assets")}\""); // 旧版本支持

            args.Append(gameArgs);
        }
        else if (versionInfo.Arguments?.Game != null)
        {
            // 新版本参数格式
            foreach (var arg in versionInfo.Arguments.Game)
            {
                if (arg is string strArg)
                {
                    var formattedArg = strArg
                        .Replace("${auth_player_name}", options.Username)
                        .Replace("${version_name}", versionInfo.Id)
                        .Replace("${game_directory}", $"\"{gameDir}\"")
                        .Replace("${assets_root}", $"\"{Path.Combine(options.MinecraftDirectory, "assets")}\"")
                        .Replace("${assets_index_name}", versionInfo.AssetIndex?.Id ?? "legacy")
                        .Replace("${auth_uuid}", options.UUID)
                        .Replace("${auth_access_token}", options.AccessToken)
                        .Replace("${user_type}", "mojang") // TODO: 支持不同的认证类型
                        .Replace("${version_type}", versionInfo.Type)
                        .Replace("${user_properties}", "{}") // TODO: 支持用户属性
                        .Replace("${game_assets}", $"\"{Path.Combine(options.MinecraftDirectory, "assets")}\"");

                    args.Append($"{formattedArg} ");
                }
                // 复杂参数格式（带条件）的处理，这里简化为直接添加
                // TODO: 添加规则判断
            }
        }

        // 添加额外的游戏参数
        if (options.ExtraGameArgs != null)
        {
            foreach (var arg in options.ExtraGameArgs)
            {
                args.Append($"{arg.Key} {arg.Value} ");
            }
        }

        return args.ToString().TrimEnd();
    }

    /// <summary>
    /// 获取游戏日志
    /// </summary>
    public ReadOnlyObservableCollection<LogEntry> GetGameLogs()
    {
        return _gameLog.Entries;
    }

    /// <summary>
    /// 清除游戏日志
    /// </summary>
    public void ClearGameLogs()
    {
        _gameLog.Clear();
    }

    /// <summary>
    /// 导出游戏日志到文件
    /// </summary>
    public async Task ExportGameLogsAsync(string filePath)
    {
        var logs = new StringBuilder();
        foreach (var entry in _gameLog.Entries)
        {
            var prefix = entry.IsError ? "[ERROR]" : "[INFO]";
            logs.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} {prefix} {entry.Message}");
        }

        await File.WriteAllTextAsync(filePath, logs.ToString());
    }
}