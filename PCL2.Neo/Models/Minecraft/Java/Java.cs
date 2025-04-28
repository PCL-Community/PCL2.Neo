using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Models.Minecraft.Java;

/// <summary>
/// 测试
/// </summary>
public sealed class Java
{
    public const int JavaListCacheVersion = 0; // [INFO] Java 缓存版本号，大版本更新后应该增加
    public bool IsInitialized { get; private set; } = false;

    public List<JavaEntity> JavaList { get; private set; } = [];

    private Java() { } // 私有构造函数

    /// <summary>
    /// 供外部调用，根据实际情况创建 Java 管理器实例
    /// </summary>
    /// <returns></returns>
    public static async Task<Java?> CreateAsync()
    {
        var java = new Java();
        await java.JavaListInit();
        return java;
    }

    /// <summary>
    /// 初始化 Java 列表，但除非没有 Java，否则不进行检查。
    /// TODO)) 更换为 Logger.cs 中的 logger
    /// </summary>
    private async Task JavaListInit()
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
        var entity = await JavaEntity.CreateJavaEntityAsync(javaDir, true);
        if (entity is { Compability: not JavaCompability.Error }) JavaList.Add(entity);
        else Console.WriteLine("添加的 Java 文件无法运行！");
    }

    public async Task Refresh()
    {
        if (!IsInitialized) return;
        IsInitialized = false;
        Console.WriteLine("[Java] 正在刷新 Java");
        List<JavaEntity> newEntities = [];
        // 对于用户手动导入的 Java，保留并重新检查可用性
        var oldManualEntities = JavaList.FindAll(entity => entity.IsUserImport);
        JavaList.Clear();
        var searchedEntities = (await SearchJava()).ToList();
        newEntities.AddRange(searchedEntities);
        foreach (JavaEntity entity in oldManualEntities)
            if (searchedEntities.All(javaEntity => javaEntity.DirectoryPath != entity.DirectoryPath))
                if(await entity.RefreshInfo())
                    newEntities.Add(entity);
        JavaList = newEntities;
        Console.WriteLine("[Java] 刷新 Java 完成");
        IsInitialized = true;
        TestOutput();
    }

    public static async Task<IEnumerable<JavaEntity>> SearchJava()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return await Windows.SearchJavaAsync();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return await Unix.SearchJavaAsync(OSPlatform.Linux);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return await Unix.SearchJavaAsync(OSPlatform.OSX);

        throw new PlatformNotSupportedException();
    }

    public void TestOutput()
    {
        if (!IsInitialized) return;
        Console.WriteLine("当前有 " + JavaList.Count + " 个 Java");
        foreach (JavaEntity? javaEntity in JavaList)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("路径: " + javaEntity.DirectoryPath);
            Console.WriteLine("是否兼容: " + javaEntity.Compability);
            Console.WriteLine("是否通用: " + javaEntity.IsFatFile);
        }
    }
}