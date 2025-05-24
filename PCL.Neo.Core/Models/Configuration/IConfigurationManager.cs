
using System.Text.Json;

namespace PCL.Neo.Core.Models.Configuration;

public interface IConfigurationManager
{
    /// <summary>
    /// 获取配置项的
    /// </summary>
    /// <typeparam name="TResult">目标反序列化的数据类</typeparam>
    /// <returns>返回反序列化后的配置结果</returns>
    TResult GetConfiguration<TResult>() where TResult : class, new();

    /// <summary>
    /// 更新配置项的值
    /// </summary>
    /// <typeparam name="TResult">目标更新配置项的数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    Task UpdateConfiguration<TResult>(TResult config, JsonSerializerOptions? options) where TResult : class, new();

    /// <summary>
    /// 创建配置文件
    /// </summary>
    /// <typeparam name="TResult">配置项数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    Task CreateCOnfiguration<TResult>(TResult config, JsonSerializerOptions? options) where TResult : class, new();
}