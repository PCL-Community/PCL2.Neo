using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data
{
    internal class YggdrasilAgent
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Minecraft";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;
    }
}