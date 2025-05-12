using System.Text.Json.Serialization;

// TODO)) 不知道这个Rule干嘛的

namespace PCL.Neo.Core.Models.Minecraft.Game.Data
{
    public class Rule
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("os")]
        public OsRule? Os { get; set; }
    }

    public class OsRule
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arch")]
        public string? Arch { get; set; }
    }
}