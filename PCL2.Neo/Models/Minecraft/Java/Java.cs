using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Models.Minecraft.Java;

/// <summary>
/// 测试
/// </summary>
public class Java
{
    public const int JavaListCacheVersion = 0;    // [INFO] Java 缓存版本号，大版本更新后应该增加
    public List<JavaEntity> JavaList { get; private set; } = [];

    private Java() { }  // 私有构造函数

    /// <summary>
    /// 供外部调用，根据实际情况创建 Java 管理器实例
    /// </summary>
    /// <returns></returns>
    public static async Task<Java> CreateAsync()
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
        JavaList = [];
        try
        {
            // TODO)) 如果本地缓存中已有 Java 列表则读取缓存
            int readJavaListCacheVersion = 0;   // TODO)) 此数字应该从缓存中读取
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
            }
            else
            {
                Console.WriteLine($"[Java] 缓存中有{JavaList.Count}个可用的 Java：");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("初始化 Java 失败");
            throw;
        }
    }

    public async Task ManualAdd(string javaDir)
    {
        var entity = await JavaEntity.CreateJavaEntityAsync(javaDir);
        if(entity.Compability is not JavaCompability.Error) JavaList.Add(entity);
        else Console.WriteLine("添加的 Java 文件无法运行！");
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

    public static async Task TestOutput()
    {
        Java javaInstance = await CreateAsync();
        Console.WriteLine("当前有 " + javaInstance.JavaList.Count + " 个 Java");
        foreach (JavaEntity javaEntity in javaInstance.JavaList)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("路径: " + javaEntity.DirectoryPath);
            Console.WriteLine("是否兼容: " + javaEntity.Compability);
            Console.WriteLine("是否通用: " + javaEntity.IsFatFile);
        }
    }
}