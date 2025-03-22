using Avalonia.Styling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;

#pragma warning disable CA1416

namespace PCL2.Neo.Models.Minecraft.JavaSearcher;

internal class Windows
{
    private static Task<JavaExist> PathEnvSearchAsync(string path) => Task.Run(() => new JavaExist
    {
        IsExist = File.Exists(Path.Combine(path, "javaw.exe")), Path = path
    });

    private static Task<List<JavaEntity>> EnvionmentJavaEntities()
    {
        var javaList = new List<JavaEntity>();

        // search from environment path
        // JAVA_HOME
        var javaHomePath = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (Directory.Exists(javaHomePath))
        {
            javaList.Add(new JavaEntity(Path.Combine(javaHomePath, "bin")));
        }

        // PATH
        var result = Environment.GetEnvironmentVariable("Path")!.Split(';')
            .Select(async item => await PathEnvSearchAsync(item)).Select(it => it.Result).Where(it => it.IsExist)
            .Select(it => new JavaEntity(it.Path));

        javaList.AddRange(result);

        return Task.FromResult(javaList);
    }

    private static readonly string[] KeySubFolderWrods =
    [
        "java", "jdk", "env", "环境", "run", "软件", "jre", "mc", "dragon",
        "soft", "cache", "temp", "corretto", "roaming", "users", "craft", "program", "世界", "net",
        "游戏", "oracle", "game", "file", "data", "jvm", "服务", "server", "客户", "client", "整合",
        "应用", "运行", "前置", "mojang", "官启", "新建文件夹", "eclipse", "microsoft", "hotspot",
        "runtime", "x86", "x64", "forge", "原版", "optifine", "官方", "启动", "hmcl", "mod",
        "download", "launch", "程序", "path", "version", "baka", "pcl", "zulu", "local", "packages", "国服", "网易", "ext",
        "netease", "启动"
    ];

    public const int MaxDeep = 7;

    private static IEnumerable<JavaEntity> SearchFolders(string folderPath, int deep, int maxDeep = MaxDeep)
    {
        // if too deep then return
        if (deep >= maxDeep) return [];

        var entities = new List<JavaEntity>();

        if (File.Exists(Path.Combine(folderPath, "javaw.exe"))) entities.Add(new JavaEntity(folderPath));

        try
        {
            var targetFolders = Directory.GetDirectories(folderPath)
                .Where(f => KeySubFolderWrods.Any(w => f.Contains(w.ToLower())));

            entities.AddRange(targetFolders.Select(it => SearchFolders(it, deep + 1)).SelectMany(it => it));
        }
        catch (UnauthorizedAccessException)
        {
            // ignore can not access folder
        }

        return entities;
    }

    private static Task<IEnumerable<JavaEntity>> SearchFoldersAsync(string folderPath, int deep = 0,
        int maxDeep = MaxDeep) =>
        Task.Run(() => SearchFolders(folderPath, deep, maxDeep));

    private static Task<IEnumerable<JavaEntity>> DriveJavaEntities(int maxDeep)
    {
        var readyDrive = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });
        var readyRootFolders = readyDrive.Select(d => d.RootDirectory)
            .Where(f => !f.Attributes.HasFlag(FileAttributes.ReparsePoint));

        // search java start at root folders
        return Task.FromResult(readyRootFolders
            .Select(async item => await SearchFoldersAsync(item.FullName, 0, maxDeep))
            .SelectMany(it => it.Result));
    }

    private static IEnumerable<JavaEntity> RegisterSearch()
    {
        // JavaSoft
        using var javaSoftKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft");
        if (javaSoftKey == null) return [];

        var javaList = new List<JavaEntity>();

        foreach (var subKeyName in javaSoftKey.GetSubKeyNames())
        {
            using var subKey = javaSoftKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree);

            var javaHoemPath = subKey?.GetValue("JavaHome")?.ToString();
            if (javaHoemPath == null) continue;

            var exePath = Path.Combine(javaHoemPath, "bin", "javaw.exe");
            if (File.Exists(exePath)) javaList.Add(new JavaEntity(Path.Combine(javaHoemPath, "bin")));
        }

        return javaList;
    }

    public static async Task<List<JavaEntity>> SearchJavaAsync(bool fullSearch = false, int maxDeep = MaxDeep)
    {
        var javaEntities = new List<JavaEntity>();

        javaEntities.AddRange(RegisterSearch()); // search register
        javaEntities.AddRange(await EnvionmentJavaEntities()); // search environment

        if (fullSearch) javaEntities.AddRange(await DriveJavaEntities(maxDeep)); // full search
        else
        {
            string[] searchPath =
            [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime")
            ];

            foreach (var item in searchPath)
                if (Directory.Exists(item))
                    javaEntities.AddRange(await SearchFoldersAsync(item, maxDeep: 6));
        }

        return javaEntities;
    }
}

/// <summary>
/// 处理Unix系统下的java
/// </summary>
internal class Unix
{
    public static List<JavaEntity> SearchJavaAsync(PlatformID platform)
    {
        string[] searchPath;

        // make search path by platform
        switch (platform)
        {
            case PlatformID.Unix:
                searchPath =
                [
                    "/usr/lib/jvm",
                    "/usr/java",
                    "/opt"
                ];
                break;
            case PlatformID.MacOSX:
                searchPath =
                [
                    "/Library/Java/JavaVirtualMachines",
                    "/usr/local/Caskroom",
                    "/usr/local/opt/openjdk",
                    "/opt"
                ];
                break;
            default:
                return [];
        }

        var javaList = new List<JavaEntity>();

        foreach (var item in searchPath)
        {
            // ignore if not exist
            if (!Directory.Exists(item)) continue;

            try
            {
                javaList.AddRange(Directory.EnumerateDirectories(item)
                    .Select(jvmDir => new { jvmDir, javaExecutable = FindJavaExecuteable(jvmDir) })
                    .Where(@t => !string.IsNullOrEmpty(@t.javaExecutable))
                    .Select(@t => new JavaEntity(@t.jvmDir)));
            }
            catch (UnauthorizedAccessException)
            {
                // ignore
            }
        }

        return javaList;
    }

    private static string? FindJavaExecuteable(string jvmPath)
    {
        string[] possiblePaths =
        [
            Path.Combine(jvmPath, "bin", "java"),
            Path.Combine(jvmPath, "Contents", "Home", "bin", "java")
        ];

        return possiblePaths.FirstOrDefault(File.Exists);
    }
}