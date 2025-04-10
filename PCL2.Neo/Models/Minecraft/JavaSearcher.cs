using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

#pragma warning disable CA1416

namespace PCL2.Neo.Models.Minecraft.JavaSearcher;

internal class Windows
{
    private static Task<JavaExist> PathEnvSearchAsync(string path) => Task.Run(() => new JavaExist
        { IsExist = File.Exists(Path.Combine(path, "javaw.exe")), Path = path });

    private static async Task<List<JavaEntity>> EnvionmentJavaEntities()
    {
        var javaList = new List<JavaEntity>();

        // find by environment path
        // JAVA_HOME
        var javaHomePath = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (javaHomePath != null || Directory.Exists(javaHomePath)) // if not exist then return
            if (Directory.Exists(javaHomePath))
            {
                var filePath = javaHomePath.EndsWith(@"\bin\") ? javaHomePath : Path.Combine(javaHomePath, "bin");
                javaList.Add(new JavaEntity(filePath));
            }

        // PATH multi-thread
        var pathList = new ConcurrentBag<JavaExist>();
        foreach (var item in Environment.GetEnvironmentVariable("Path")!.Split(';'))
            pathList.Add(await PathEnvSearchAsync(item));

        javaList.AddRange(pathList.Where(j => j.IsExist).Select(j => new JavaEntity(j.Path)));

        return javaList;
    }

    private static readonly string[] KeySubFolderWrods =
    [
        "java", "jdk", "env", "环境", "run", "软件", "jre", "mc", "dragon",
        "soft", "cache", "temp", "corretto", "roaming", "users", "craft", "program", "世界", "net",
        "游戏", "oracle", "game", "file", "data", "jvm", "服务", "server", "客户", "client", "整合",
        "应用", "运行", "前置", "mojang", "官启", "新建文件夹", "eclipse", "microsoft", "hotspot",
        "runtime", "x86", "x64", "forge", "原版", "optifine", "官方", "启动", "hmcl", "mod", "高清",
        "download", "launch", "程序", "path", "version", "baka", "pcl", "zulu", "local", "packages",
        "4297127d64ec6", "国服", "网易", "ext", "netease", "1.", "启动"
    ];

    public const int MaxDeep = 7;

    private static List<JavaEntity> SearchFolders(string folderPath, int deep, int maxDeep = MaxDeep)
    {
        var entities = new List<JavaEntity>();
        // if too deep then return
        if (deep >= maxDeep) return entities;

        try
        {
            if (File.Exists(Path.Combine(folderPath, "javaw.exe"))) entities.Add(new JavaEntity(folderPath));

            var subFolder = Directory.GetDirectories(folderPath);

            var selectFolder = subFolder.Where(f => KeySubFolderWrods.Any(w => f.ToLower().Contains(w.ToLower())));
            //entities.AddRange(selectFolder.Select(SearchFolders).SelectMany(i => i).ToList());
            foreach (var folder in selectFolder)
                entities.AddRange(SearchFolders(folder, deep + 1)); // search sub folders
        }
        catch (UnauthorizedAccessException)
        {
            // ignore can not access folder
        }

        return entities;
    }

    private static Task<List<JavaEntity>> SearchFoldersAsync(string folderPath, int deep = 0, int maxDeep = MaxDeep) =>
        Task.Run(() => SearchFolders(folderPath, deep, maxDeep));

    private static async Task<List<JavaEntity>> DriveJavaEntities(int maxDeep)
    {
        var javaList = new ConcurrentBag<JavaEntity>();

        var readyDrive = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });
        var readyRootFolders = readyDrive.Select(d => d.RootDirectory)
            .Where(f => !f.Attributes.HasFlag(FileAttributes.ReparsePoint));

        // search java start at root folders
        // multi-thread
        foreach (var item in readyRootFolders)
        {
            var entities = await SearchFoldersAsync(item.ToString(), maxDeep: maxDeep);
            foreach (var entity in entities) javaList.Add(entity);
        }

        return javaList.ToList();
    }

    public static List<JavaEntity> RegisterSearch()
    {
        // JavaSoft
        using var javaSoftKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft");
        if (javaSoftKey == null) return [];

        var javaList = new List<JavaEntity>();

        foreach (var subKeyName in javaSoftKey.GetSubKeyNames())
        {
            using var subKey = javaSoftKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree);

            var javaHome = subKey?.GetValue("JavaHome");

            var javaHoemPath = javaHome?.ToString();
            if (javaHoemPath == null) continue;

            var exePath = Path.Combine(javaHoemPath, "bin", "javaw.exe");
            if (File.Exists(exePath)) javaList.Add(new JavaEntity(Path.Combine(javaHoemPath, "bin")));
        }

        return javaList;
    }

    public static async Task<List<JavaEntity>> SearchJava(bool fullSearch = false, int maxDeep = MaxDeep)
    {
        var javaEntities = new List<JavaEntity>();
        javaEntities.AddRange(RegisterSearch()); // search register
        javaEntities.AddRange(await EnvionmentJavaEntities()); // search environment
        if (fullSearch) javaEntities.AddRange(await DriveJavaEntities(maxDeep)); // full search
        else
        {
            var programFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java");
            var programFileX86 =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java");
            javaEntities.AddRange(await SearchFoldersAsync( // search minecraf launcher runtime folder
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime"),
                maxDeep: 6));
            if (Directory.Exists(programFile)) // search program java
                javaEntities.AddRange(await SearchFoldersAsync(programFile, maxDeep: 4));
            if (Directory.Exists(programFileX86)) // search program x86 java
                javaEntities.AddRange(await SearchFoldersAsync(programFileX86, maxDeep: 4));
        }

        return javaEntities;
    }
}

/// <summary>
/// 处理Unix系统下的java
/// </summary>
internal class Unix
{
    public static List<JavaEntity> SerachJava()
    {
        var javaList = new List<JavaEntity>();

        // TODO: Add code

        return javaList;
    }
}
