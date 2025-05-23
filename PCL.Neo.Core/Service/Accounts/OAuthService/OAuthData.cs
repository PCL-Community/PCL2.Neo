using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

public static class OAuthData
{
    public static class RequestUrls
    {
        /// <summary>
        /// 获取授权码模式下的授权码地址
        /// </summary>
        public static readonly Uri AuthCodeUri =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize");

        /// <summary>
        /// 获取设备码模式下的授权码地址
        /// </summary>
        public static readonly Uri DeviceCode =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode");

        /// <summary>
        /// 获取令牌
        /// </summary>
        public static readonly Uri TokenUri =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/token");

        /// <summary>
        /// XboxLive验证地址
        /// </summary>
        public static readonly Uri XboxLiveAuth =
            new("https://user.auth.xboxlive.com/user/authenticate");

        /// <summary>
        /// Xsts验证地址
        /// </summary>
        public static readonly Uri XstsAuth =
            new("https://xsts.auth.xboxlive.com/xsts/authorize");

        /// <summary>
        /// Mc通行令牌获取地址
        /// </summary>
        public static readonly Uri MinecraftAccessTokenUri =
            new("https://api.minecraftservices.com/authentication/login_with_xbox");

        /// <summary>
        /// 检查是否拥有Mc地址
        /// </summary>
        public static readonly Uri CheckHasMc =
            new("https://api.minecraftservices.com/entitlements/mcstore");

        /// <summary>
        /// 获取玩家UUID的地址
        /// </summary>
        public static readonly Uri PlayerUuidUri =
            new("https://api.minecraftservices.com/minecraft/profile");
    }

    public static class FormUrlReqData
    {
        // TODO: 配置微软OAuth客户端ID
        // replaceed by config modul, follwed are same
        public const string ClientId = "";

        // TODO: 配置微软OAuth重定向URI
        public static readonly Uri RedirectUri = new("http://127.0.0.1:5050");

        // TODO: 配置微软OAuth客户端密钥
        public const string ClientSecret = "";

        /// <summary>
        /// 获取授权码的地址
        /// </summary>
        /// <returns>地址</returns>
        public static string GetAuthCodeData() =>
            $"{RequestUrls.AuthCodeUri}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope=XboxLive.signin offline_access";

        /// <summary>
        /// 设备码申请参数
        /// </summary>
        public static IReadOnlyDictionary<string, string> DeviceCodeData { get; } =
            new Dictionary<string, string> { { "client_id", ClientId }, { "scope", "XboxLive.signin offline_access" } }
                .ToImmutableDictionary();

        /// <summary>
        /// 用户授权状态查询参数
        /// </summary>
        public static IReadOnlyDictionary<string, string> UserAuthStateData { get; } =
            new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                { "client_id", ClientId },
                { "device_code", "" }
            }.ToImmutableDictionary();

        /// <summary>
        /// 授权令牌参数
        /// </summary>
        public static IReadOnlyDictionary<string, string> AuthTokenData { get; } =
            new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "code", "" },
                { "grant_type", "authorization_code" },
                { "redirect_uri", RedirectUri.ToString() },
                { "scope", "XboxLive.signin offline_access" }
            }.ToImmutableDictionary();

        /// <summary>
        /// 刷新令牌参数
        /// </summary>
        public static IReadOnlyDictionary<string, string> RefreshTokenData { get; } =
            new Dictionary<string, string>
            {
                { "client_id", ClientId },
                { "client_secret", ClientSecret },
                { "refresh_token", "" },
                { "grant_type", "refresh_token" },
                { "scope", "XboxLive.signin offline_access" }
            }.ToImmutableDictionary();
    }

    public static class RequireData
    {
        public sealed record XboxLiveAuthRequire
        {
            [property: JsonPropertyName("PropertiesData")]
            public PropertiesData Properties { get; set; }

            public const  string TokenType = "JWT";
            public static string RelyingParty => "http://auth.xboxlive.com";

            public sealed record PropertiesData(
                [property: JsonPropertyName("RpsTicket")]
                string RpsTicket)
            {
                public const string AuthMethod = "RPS";
                public const string SiteName   = "user.auth.xboxlive.com";
            }
        }

        public sealed record XstsRequire(
            XstsRequire.PropertiesData Properties)
        {
            public const string RelyingParty = "rp://api.minecraftservices.com/";
            public const string TokenType    = "JWT";

            public sealed record PropertiesData(
                [property: JsonPropertyName("UserTokens")]
                List<string> UserTokens)
            {
                public const string SandboxId = "RETAIL";
            }
        }

        public sealed record MinecraftAccessTokenRequire
        {
            [JsonPropertyName("identityToken")] public string IdentityToken { get; set; }
        }
    }

    public static class ResponseData
    {
        public sealed record AccessTokenResponse
        {
            [JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }

            [JsonPropertyName("ext_expires_in")]
            public required int ExtExpiresIn { get; set; }

            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public required string RefreshToken { get; set; }
        }

        public sealed record UserAuthStateResponse
        {
            [property: JsonPropertyName("expires_in")]
            public int? ExpiresIn { get; set; }

            [property: JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [property: JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }

            [property: JsonPropertyName("error")]
            public string? Error { get; set; }

            [property: JsonPropertyName("error_description")]
            public string? ErrorDescription { get; set; }

            [property: JsonPropertyName("correlation_id")]
            public string? CorrelationId { get; set; }
        }

        public sealed record XboxResponse
        {
            /*
                        [JsonPropertyName("IssueInstant")] public DateTime IssueInstant { get; set; }
            */
            /*
                        [JsonPropertyName("NotAfter")] public DateTime NotAfter { get; set; }
            */
            [JsonPropertyName("Token")]
            public required string Token { get; set; }

            [JsonPropertyName("DisplayClaims")]
            public required DisplayClaimsData DisplayClaims { get; set; }

            public record DisplayClaimsData
            {
                [JsonPropertyName("xui")]
                public required List<XuiData> Xui { get; set; }

                public record XuiData
                {
                    [JsonPropertyName("uhs")]
                    public required string Uhs { get; set; }
                }
            }
        }

        public sealed record MinecraftAccessTokenResponse
        {
            [JsonPropertyName("username")]
            public required string Username { get; set; }

            [JsonPropertyName("roles")]
            public required List<object> Roles { get; set; }

            [JsonPropertyName("access_token")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("token_type")]
            public required string TokenType { get; set; }

            [JsonPropertyName("expires_in")]
            public required int ExpiresIn { get; set; }
        }

        public sealed record CheckHaveGameResponse
        {
            [JsonPropertyName("items")]
            public required List<Item> Items { get; set; }
            /*
                        [JsonPropertyName("signature")] public string Signature { get; set; }
            */
            /*
                        [JsonPropertyName("keyId")] public string KeyId { get; set; }
            */

            public sealed record Item
            {
                [JsonPropertyName("name")]
                public required string Name { get; set; }

                [JsonPropertyName("signature")]
                public string? Signature { get; set; }
            }
        }

        public sealed record MinecraftPlayerUuidResponse
        {
            [JsonPropertyName("id")]
            public required string Uuid { get; set; }

            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("skins")]
            public List<Skin>? Skins { get; set; }

            [JsonPropertyName("capes")]
            public List<Cape>? Capes { get; set; }

            public sealed record Skin
            {
                [JsonPropertyName("id")]
                public required string Id { get; set; }

                [JsonPropertyName("state")]
                public required string State { get; set; }

                [JsonPropertyName("url")]
                public required string Url { get; set; }

                [JsonPropertyName("variant")]
                public required string Variant { get; set; }

                [JsonPropertyName("textureKey")]
                public required string TextureKey { get; set; }

                [JsonPropertyName("alias")]
                public string? Alias { get; set; }
            }

            public sealed record Cape
            {
                [JsonPropertyName("id")]
                public required string Id { get; set; }

                [JsonPropertyName("state")]
                public required string State { get; set; }

                [JsonPropertyName("url")]
                public required string Url { get; set; }

                [JsonPropertyName("alias")]
                public string? Alias { get; set; }
            }
        }
    }
}