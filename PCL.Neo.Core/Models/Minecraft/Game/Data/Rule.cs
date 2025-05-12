using PCL.Neo.Core.Utils;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public class OsRule
{
    [JsonPropertyName("name")] public SystemUtils.RunningOs Name { get; set; }

    [JsonPropertyName("arch")] public Architecture? Arch { get; set; }
}

public class ArgRule
{
    [JsonPropertyName("allow")] public bool Allow { get; set; }

    [JsonPropertyName("features")] public Dictionary<string, bool>? Features { get; set; }

    [JsonPropertyName("os")] public OsRule? Os { get; set; }
}

public class Rule
{
    [JsonPropertyName("allow")] public bool Allow { get; set; }

    [JsonPropertyName("os")] public OsRule? Os { get; set; }

    private bool IsOsRuleAllow
    {
        get
        {
            if (Os?.Name is null) return true;
            bool isCurrentOs = this.Os.Name == SystemUtils.Os;
            return (isCurrentOs && Allow) || (!isCurrentOs && !Allow);
        }
    }

    private bool IsArchRuleAllow
    {
        get
        {
            if (Os?.Arch is null) return true;
            bool isCurrentArch = Os.Arch == SystemUtils.Architecture;
            return (isCurrentArch && Allow) || (!isCurrentArch && !Allow);
        }
    }

    private bool IsGameFeatureAllow => true; // TODO)) 设置具体值
    private bool GameArgumentsFilter => IsGameFeatureAllow || IsOsRuleAllow || IsArchRuleAllow;
    private bool JvmArgumentsFilter => IsOsRuleAllow || IsArchRuleAllow;
}