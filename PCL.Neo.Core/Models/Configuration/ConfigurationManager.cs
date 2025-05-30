using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Models.Configuration;

public class ConfigurationManager : IConfigurationManager
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
    };

    /// <inheritdoc />
    public TResult? GetConfiguration<TResult>() where TResult : class, new()
    {
        try
        {
            var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();

            if (attribute == null)
            {
                return null;
            }

            // 获取配置路径，优先使用GlobalSettings中的路径
            string configPath = GetConfigPath<TResult>(attribute.FilePath);
            
            // 确保配置目录存在
            string? directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(configPath))
            {
                return null;
            }

            var jsonContent = File.ReadAllText(configPath);
            var result = JsonSerializer.Deserialize<TResult>(jsonContent);

            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateConfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new()
    {
        try
        {
            var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();
            if (attribute == null)
            {
                return false;
            }

            // 获取配置路径，优先使用GlobalSettings中的路径
            string configPath = GetConfigPath<TResult>(attribute.FilePath);
            
            // 确保配置目录存在
            string? directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(config, options ?? DefaultOptions);
            await File.WriteAllTextAsync(configPath, jsonContent);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateConfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new()
    {
        try
        {
            var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();

            if (attribute == null)
            {
                return false;
            }

            // 获取配置路径，优先使用GlobalSettings中的路径
            string configPath = GetConfigPath<TResult>(attribute.FilePath);
            
            // 确保配置目录存在
            string? directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var content = JsonSerializer.Serialize(config, options ?? DefaultOptions);
            await File.WriteAllTextAsync(configPath, content);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取配置文件路径，优先使用GlobalSettings中的配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="attributePath">特性中指定的路径</param>
    /// <returns>最终使用的配置路径</returns>
    private string GetConfigPath<T>(string attributePath)
    {
        // 特殊处理已知的配置类型
        if (typeof(T).Name == "AppSettings")
        {
            return GlobalSettings.GetConfigFilePath(GlobalSettings.AppSettingsFile);
        }
        else if (typeof(T).Name == "OAuth2Configurations")
        {
            return GlobalSettings.GetConfigFilePath(GlobalSettings.OAuth2ConfigurationFile);
        }
        
        // 默认使用特性中的路径
        return attributePath.Contains(Path.DirectorySeparatorChar) || attributePath.Contains(Path.AltDirectorySeparatorChar)
            ? attributePath // 已经是完整路径
            : GlobalSettings.GetConfigFilePath(attributePath); // 只是文件名，需要添加路径
    }

    /// <summary>
    /// 获取配置，如果不存在则创建默认配置
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <returns>配置对象</returns>
    public async Task<TResult> GetOrCreateConfiguration<TResult>() where TResult : class, new()
    {
        var config = GetConfiguration<TResult>();
        if (config == null)
        {
            config = new TResult();
            await CreateConfiguration(config, null);
        }
        return config;
    }

    /// <summary>
    /// 从指定路径加载配置
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>配置对象，失败返回null</returns>
    public TResult? LoadFromPath<TResult>(string filePath) where TResult : class, new()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonContent = File.ReadAllText(filePath);
            var result = JsonSerializer.Deserialize<TResult>(jsonContent);
            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 保存配置到指定路径
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <param name="config">配置对象</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="options">序列化选项</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SaveToPath<TResult>(TResult config, string filePath, JsonSerializerOptions? options = null) 
        where TResult : class, new()
    {
        try
        {
            // 确保配置目录存在
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var content = JsonSerializer.Serialize(config, options ?? DefaultOptions);
            await File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static readonly ConfigurationManager Instance = new();
}