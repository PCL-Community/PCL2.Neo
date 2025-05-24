using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace PCL.Neo.Core.Models.Configuration;

public class ConfigManager(IConfigurationRoot root) : IConfigManager
{
    private IConfigurationRoot ConfigurationRoot { get; } = root;

    /// <inheritdoc />
    public TResult GetConfiguration<TResult>() where TResult : class, new()
    {
        var attribute = typeof(TResult)
            .GetCustomAttribute<ConfigInfoAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(TResult).FullName} must be decorated white [ConfigInfoAttribute].");
        }

        var config = ConfigurationRoot.GetSection(attribute.Path).Get<TResult>();
        return config ?? throw new ArgumentNullException(nameof(TResult),
            $"Configuration for {typeof(TResult).Name} not found.");
    }

    /// <inheritdoc />
    [Obsolete("每次都会创建一个新的IConfigationRoot，有性能问题")]
    public TResult GetConfigurationFromNewFile<TResult>() where TResult : class, new()
    {
        var attribute = typeof(TResult)
            .GetCustomAttribute<ConfigInfoAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(TResult).FullName} must be decorated white [ConfigInfoAttribute].");
        }

        var config = new ConfigurationBuilder()
            .AddJsonFile(attribute.FileName, optional: false, reloadOnChange: true)
            .Build()
            .GetSection(attribute.Path)
            .Get<TResult>();

        return config ?? throw new ArgumentNullException(nameof(TResult),
            $"Configuration for {typeof(TResult).Name} not found.");
    }
}