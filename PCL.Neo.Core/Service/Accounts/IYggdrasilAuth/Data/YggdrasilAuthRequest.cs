using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data
{
    internal class YggdrasilAuthRequest
    {
        [JsonPropertyName("agent")]
        public YggdrasilAgent Agent { get; set; } = new YggdrasilAgent();

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("clientToken")]
        public string ClientToken { get; set; } = string.Empty;

        [JsonPropertyName("requestUser")]
        public bool RequestUser { get; set; } = true;
    }
}