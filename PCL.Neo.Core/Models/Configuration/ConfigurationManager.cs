using System.Reflection;
using System.Text.Json;

namespace PCL.Neo.Core.Models.Configuration;

public class ConfigurationManager : IConfigurationManager
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
    };

    /// <inheritdoc />
    public TResult GetConfiguration<TResult>() where TResult : class, new()
    {
        var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException("Configuration attribute not found for the specified type.");
        }

        if (!File.Exists(attribute.FilePath))
        {
            throw new FileNotFoundException($"Target file {attribute.FilePath} not found.");
        }

        var jsonContent = File.ReadAllText(attribute.FilePath);
        var result      = JsonSerializer.Deserialize<TResult>(jsonContent);

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize configuration.");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task UpdateConfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new()
    {
        var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();
        if (attribute == null)
        {
            throw new InvalidOperationException("Configuration attribute not found for the specified type.");
        }

        var jsonContent = JsonSerializer.Serialize(config, options ?? DefaultOptions);
        await File.WriteAllTextAsync(attribute.FilePath, jsonContent);
    }

    /// <inheritdoc />
    public async Task CreateCOnfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new()
    {
        var attribute = typeof(TResult).GetCustomAttribute<ConfigurationInfoAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException("Configuration attribute not found for the specified type.");
        }

        var content = JsonSerializer.Serialize(config, options ?? DefaultOptions);
        await File.WriteAllTextAsync(attribute.FilePath, content);
    }


    public static readonly ConfigurationManager Instance = new();
}