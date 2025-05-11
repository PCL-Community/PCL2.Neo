using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace PCL.Neo.Core.Models.Minecraft.Java;

/// <summary>
/// JavaManager工厂类，用于依赖注入容器中注册JavaManager服务
/// </summary>
public static class JavaManagerFactory
{
    /// <summary>
    /// 添加Java管理服务到依赖注入容器
    /// </summary>
    public static IServiceCollection AddJavaManager(this IServiceCollection services)
    {
        // 注册JavaManager作为单例服务
        services.AddSingleton<IJavaManager, JavaManager>();
        return services;
    }
    
    /// <summary>
    /// 配置Java管理服务
    /// </summary>
    public static IServiceCollection ConfigureJavaManager(this IServiceCollection services, Action<JavaManager> configureAction)
    {
        // 获取JavaManager服务
        var serviceProvider = services.BuildServiceProvider();
        var javaManager = serviceProvider.GetRequiredService<IJavaManager>() as JavaManager;
        
        if (javaManager != null)
        {
            configureAction(javaManager);
        }
        
        return services;
    }
}