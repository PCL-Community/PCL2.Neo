using System.Text.Json;

namespace PCL.Neo.Core.Models.Configuration;

public interface IConfigurationManager
{
    /// <summary>
    /// 获取配置项
    /// </summary>
    /// <typeparam name="TResult">目标反序列化的数据类</typeparam>
    /// <returns>返回反序列化后的配置结果，失败返回null</returns>
    TResult? GetConfiguration<TResult>() where TResult : class, new();

    /// <summary>
    /// 更新配置项的值
    /// </summary>
    /// <typeparam name="TResult">目标更新配置项的数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateConfiguration<TResult>(TResult config, JsonSerializerOptions? options) where TResult : class, new();

    /// <summary>
    /// 创建配置文件
    /// </summary>
    /// <typeparam name="TResult">配置项数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    /// <returns>操作是否成功</returns>
    Task<bool> CreateConfiguration<TResult>(TResult config, JsonSerializerOptions? options) where TResult : class, new();
    
    /// <summary>
    /// 获取配置，如果不存在则创建默认配置
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <returns>配置对象</returns>
    Task<TResult> GetOrCreateConfiguration<TResult>() where TResult : class, new();
}