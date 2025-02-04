using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

#pragma warning disable CA1416

namespace PCL2.Neo.Models.Minecraft.JavaSearcher;

/// <summary>
/// 处理windows系统下的java
/// </summary>
internal class Windows
{
    /// <summary>
    /// 异步搜索指定路径下是否存在javaw.exe，并返回Java存在信息。
    /// </summary>
    /// <param name="path">要检查的路径。</param>
    /// <returns>返回包含路径和是否存在Java环境的信息。</returns>
    private static Task<JavaExist> PathEnvSearchAsync(string path) => Task.Run(() => new JavaExist
        { IsExist = File.Exists(Path.Combine(path, "javaw.exe")), Path = path });

    /// <summary>
    /// 从环境变量中异步查找所有可用的Java环境实体。
    /// </summary>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
    private static async Task<List<JavaEntity>> EnvionmentJavaEntities()
    {
        var javaList = new List<JavaEntity>();

        // 根据JAVA_HOME环境变量查找Java安装目录
        // find by environment path
        // JAVA_HOME
        var javaHomePath = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (javaHomePath != null || Directory.Exists(javaHomePath)) // if not exist then return
            if (Directory.Exists(javaHomePath))
            {
                // 确保路径指向bin目录或在其后追加"bin"
                var filePath = javaHomePath.EndsWith(@"\bin\") ? javaHomePath : Path.Combine(javaHomePath, "bin");
                javaList.Add(new JavaEntity(filePath));
            }

        // 根据Path环境变量多线程查找可能的Java安装位置
        // PATH multi-thread
        var pathList = new ConcurrentBag<JavaExist>();
        foreach (var item in Environment.GetEnvironmentVariable("Path")!.Split(';'))
            pathList.Add(await PathEnvSearchAsync(item));

        // 过滤出存在javaw.exe的路径并转换为JavaEntity实体
        javaList.AddRange(pathList.Where(j => j.IsExist).Select(j => new JavaEntity(j.Path)));

        return javaList;
    }

    /// <summary>
    /// 关键子文件夹名称数组，用于筛选可能包含Java环境的文件夹。
    /// </summary>
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

    /// <summary>
    /// 最大递归深度。
    /// </summary>
    public const int MaxDeep = 7;

    /// <summary>
    /// 在指定目录及其子目录中递归搜索包含javaw.exe的文件夹。
    /// </summary>
    /// <param name="folderPath">要搜索的文件夹路径。</param>
    /// <param name="deep">当前递归深度。</param>
    /// <param name="maxDeep">最大递归深度，默认值为MaxDeep。</param>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
    private static List<JavaEntity> SearchFolders(string folderPath, int deep, int maxDeep = MaxDeep)
    {
        var entities = new List<JavaEntity>();

        // 如果递归深度超过最大值，则返回
        // if too deep then return
        if (deep >= maxDeep) return entities;

        try
        {
            // 检查当前文件夹是否包含javaw.exe
            if (File.Exists(Path.Combine(folderPath, "javaw.exe"))) entities.Add(new JavaEntity(folderPath));

            // 获取所有子文件夹
            var subFolder = Directory.GetDirectories(folderPath);

            // 筛选出包含关键字的子文件夹
            var selectFolder = subFolder.Where(f => KeySubFolderWrods.Any(w => f.ToLower().Contains(w.ToLower())));
            //entities.AddRange(selectFolder.Select(SearchFolders).SelectMany(i => i).ToList());
            // 对每个选中的子文件夹进行递归搜索
            foreach (var folder in selectFolder)
                entities.AddRange(SearchFolders(folder, deep + 1)); // search sub folders
        }
        catch (UnauthorizedAccessException)
        {
            // 忽略无法访问的文件夹
            // ignore can not access folder
        }

        return entities;
    }

    /// <summary>
    /// 异步执行SearchFolders方法。
    /// </summary>
    /// <param name="folderPath">要搜索的文件夹路径。</param>
    /// <param name="deep">当前递归深度，默认为0。</param>
    /// <param name="maxDeep">最大递归深度，默认值为MaxDeep。</param>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
    private static Task<List<JavaEntity>> SearchFoldersAsync(string folderPath, int deep = 0, int maxDeep = MaxDeep) =>
        Task.Run(() => SearchFolders(folderPath, deep, maxDeep));

    /// <summary>
    /// 从驱动器根目录开始异步搜索Java环境实体。
    /// </summary>
    /// <param name="maxDeep">最大递归深度。</param>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
    private static async Task<List<JavaEntity>> DriveJavaEntities(int maxDeep)
    {
        var javaList = new ConcurrentBag<JavaEntity>();

        // 获取所有准备好的固定驱动器
        var readyDrive = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });
        var readyRootFolders = readyDrive.Select(d => d.RootDirectory)
            .Where(f => !f.Attributes.HasFlag(FileAttributes.ReparsePoint));

        // 多线程搜索Java环境
        // search java start at root folders
        // multi-thread
        foreach (var item in readyRootFolders)
        {
            var entities = await SearchFoldersAsync(item.ToString(), maxDeep: maxDeep);
            foreach (var entity in entities) javaList.Add(entity);
        }

        return javaList.ToList();
    }

    /// <summary>
    /// 在注册表中搜索Java环境实体。
    /// </summary>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
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

    /// <summary>
    /// 搜索系统中的Java环境实体。
    /// </summary>
    /// <param name="fullSearch">是否进行全面搜索，默认为false。</param>
    /// <param name="maxDeep">最大递归深度，默认值为MaxDeep。</param>
    /// <returns>返回一个包含所有找到的Java环境实体的列表。</returns>
    public static async Task<List<JavaEntity>> SearchJava(bool fullSearch = false, int maxDeep = MaxDeep)
    {
        var javaEntities = new List<JavaEntity>();
        javaEntities.AddRange(RegisterSearch()); // search register // 注册表搜索
        javaEntities.AddRange(await EnvionmentJavaEntities()); // search environment // 环境变量搜索
        if (fullSearch) javaEntities.AddRange(await DriveJavaEntities(maxDeep)); // full search // 全面搜索
        else
        {
            javaEntities.AddRange(await SearchFoldersAsync( // search minecraf launcher runtime folder // 特定路径搜索
                @"C:\Users\WhiteCAT\AppData\Local\Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime",
                maxDeep: 6));
            if (Directory.Exists(@"C:\Program Files\Java\")) // search program java // Program Files Java搜索
                javaEntities.AddRange(await SearchFoldersAsync(@"C:\Program Files\Java\", maxDeep: 4));
            if (Directory.Exists(@"C:\Program Files (x86)\Java\")) // search program x86 java // Program Files (x86) Java搜索
                javaEntities.AddRange(await SearchFoldersAsync(@"C:\Program files (x86)\Java\", maxDeep: 4));
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
