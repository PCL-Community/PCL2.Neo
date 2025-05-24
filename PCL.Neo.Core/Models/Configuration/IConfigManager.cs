using Microsoft.Extensions.Configuration;

namespace PCL.Neo.Core.Models.Configuration;

public interface IConfigManager
{
    /// <summary>
    /// 获取配置项
    /// </summary>
    /// <typeparam name="TResult">配置项对应的数据类</typeparam>
    /// <returns>获取到的配置类</returns>
    /// <exception cref="InvalidOperationException">目的数据类未被 <see cref="ConfigInfoAttribute"/> 注解</exception>
    /// <exception cref="ArgumentNullException">配置项未找到</exception>
    TResult GetConfiguration<TResult>() where TResult : class, new();

    /// <summary>
    /// 从新文件中获取配置项
    /// </summary>
    /// <typeparam name="TResult">配置项对应的数据类</typeparam>
    /// <returns>获取到的配置项</returns>
    TResult GetConfigurationFromNewFile<TResult>() where TResult : class, new();
}