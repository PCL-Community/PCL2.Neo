using PCL2.Neo.Helpers;
using PCL2.Neo.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace PCL2.Neo.Models.Minecraft.Java;

/// <summary>
/// 测试
/// </summary>
public sealed partial class JavaManager : IJavaManager
{
    public const int JavaListCacheVersion = 0; // [INFO] Java 缓存版本号，大版本更新后应该增加
    public bool IsInitialized { get; private set; } = false;

    public List<JavaRuntime> JavaList { get; private set; } = [];

    /// <summary>
    /// 初始化 Java 列表，但除非没有 Java，否则不进行检查。
    /// <remarks> TODO)) 更换为 Logger.cs 中的 logger </remarks>
    /// </summary>
    public async Task JavaListInit()
    {
        if (IsInitialized) return;
        JavaList = [];
        try
        {
            // TODO)) 如果本地缓存中已有 Java 列表则读取缓存
            int readJavaListCacheVersion = 0; // TODO)) 此数字应该从缓存中读取
            if (readJavaListCacheVersion < JavaListCacheVersion)
            {
                // TODO)) 设置本地版本号为 JavaListCacheVersion
                Console.WriteLine("[Java] 要求 Java 列表缓存更新");
            }
            else
            {
                // TODO)) 从本地缓存中读取 Java 列表
            }

            if (JavaList.Count == 0)
            {
                Console.WriteLine("[Java] 初始化未找到可用的 Java，将自动触发搜索");
                JavaList = (await SearchJava()).ToList();
                Console.Write($"[Java] 搜索完成 ");
            }
            else
            {
                Console.WriteLine($"[Java] 缓存中有{JavaList.Count}个可用的 Java：");
            }

            IsInitialized = true;
            TestOutput();
        }
        catch (Exception e)
        {
            Console.WriteLine("初始化 Java 失败");
            IsInitialized = false;
            throw;
        }
    }

    public async Task ManualAdd(string javaDir)
    {
        if (!IsInitialized) return;
        if (JavaList.FirstOrDefault(runtime => runtime.DirectoryPath == javaDir) is { } existingRuntime)
        {
            Console.WriteLine("选择的 Java 在列表中已存在，将其标记为手动导入。");
            existingRuntime.IsUserImport = true;
            return;
        }

        var entity = await JavaRuntime.CreateJavaEntityAsync(javaDir, true);
        if (entity is { Compability: not JavaCompability.Error })
        {
            JavaList.Add(entity);
            Console.WriteLine("已成功添加！");
        }
        else Console.WriteLine("添加的 Java 文件无法运行！");
    }

    public async Task Refresh()
    {
        if (!IsInitialized) return;
        IsInitialized = false;
        Console.WriteLine("[Java] 正在刷新 Java");
        List<JavaRuntime> newEntities = [];
        // 对于用户手动导入的 Java，保留并重新检查可用性
        var oldManualEntities = JavaList.FindAll(entity => entity.IsUserImport);
        JavaList.Clear();
        var searchedEntities = (await SearchJava()).ToList();
        newEntities.AddRange(searchedEntities);
        foreach (var oldRuntime in oldManualEntities.Where(entity =>
                     searchedEntities.All(javaEntity => javaEntity.DirectoryPath != entity.DirectoryPath)))
            if (await oldRuntime.RefreshInfo())
                newEntities.Add(oldRuntime);
            else
                Console.WriteLine($"[Java] 用户导入的 Java 已不可用，已自动剔除：{oldRuntime.DirectoryPath}");
        JavaList = newEntities;
        Console.WriteLine("[Java] 刷新 Java 完成");
        if (JavaList.Count == 0)
        {
            // TODO)) 提示用户未找到已安装的 java，是否自动下载合适版本，然后再下载
            var neo2SysDir = Directory.CreateDirectory(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PCL2.Neo", "Java")); // TODO)) 此处的路径等配置文件的模块写好了以后应该从配置文件中获取
            var cts = new CancellationTokenSource();
            var progress =
                new Progress<(int, int)>(value =>
                    Console.WriteLine(
                        $"下载进度：已下载{value.Item1}/总文件数{value.Item2}")); // TODO)) 后续这个 progress 可以设置成在 UI 上显示
            var fetchedJavaDir = await FetchJavaOnline(Const.Platform, neo2SysDir.FullName,
                MojangJavaVersion.Α, progress, cts.Token);
            if (fetchedJavaDir != null)
            {
                var runtime = await JavaRuntime.CreateJavaEntityAsync(fetchedJavaDir, true);
                JavaList.Add(runtime!);
            }
        }

        IsInitialized = true;
        TestOutput();
    }

    private static async Task<IEnumerable<JavaRuntime>> SearchJava()
    {
        return Const.Os switch
        {
            Const.RunningOs.Windows => await Windows.SearchJavaAsync(),
            Const.RunningOs.Linux or Const.RunningOs.MacOs => await Unix.SearchJavaAsync(Const.Os),
            _ => throw new PlatformNotSupportedException()
        };
    }

    public void TestOutput()
    {
        if (!IsInitialized) return;
        Console.WriteLine("当前有 " + JavaList.Count + " 个 Java");
        foreach (JavaRuntime? javaEntity in JavaList)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("路径: " + javaEntity.DirectoryPath);
            Console.WriteLine("是否兼容: " + javaEntity.Compability);
            Console.WriteLine("架构：" + javaEntity.Architecture);
            Console.WriteLine("发行商：" + javaEntity.Implementor);
            Console.WriteLine("版本：" + javaEntity.Version);
            Console.WriteLine("数字版本：" + javaEntity.SlugVersion);
        }
    }
}