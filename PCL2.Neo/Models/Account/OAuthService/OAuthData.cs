using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL2.Neo.Models.Account.OAuthService;

public static class OAuthData
{
#nullable disable
    public static class RequestUrls
    {
        public static readonly Uri AuthCodeUri =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize");

        public static readonly Uri DeviceCode =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode");

        public static readonly Uri TokenUri = new("https://login.microsoftonline.com/consumers/oauth2/v2.0/token");
        public static readonly Uri XboxLiveAuth = new("https://user.auth.xboxlive.com/user/authenticate");
        public static readonly Uri XstsAuth = new("https://xsts.auth.xboxlive.com/xsts/authorize");

        public static readonly Uri MinecraftAccessTokenUri =
            new("https://api.minecraftservices.com/authentication/login_with_xbox");

        public static readonly Uri CheckHasMc = new("https://api.minecraftservices.com/entitlements/mcstore");
        public static readonly Uri PlayerUuidUri = new("https://api.minecraftservices.com/minecraft/profile");
    }

    public static class FormUrlReqData
    {
        // todo: optimize this code with class for safe
        public static readonly string ClientId = string.Empty;
        public static readonly Uri RedirectUri = new("http://127.0.0.1:5050"); // TODO: update Uri
        public static readonly string ClientSecret = string.Empty; // TODO: Set client secret

        public static string AuthCodeData =
            new(
                $"{RequestUrls.AuthCodeUri}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope=XboxLive.signin offline_access");

        public static readonly Dictionary<string, string> DeviceCodeData = new()
        {
            { "client_id", ClientId }, { "scope", "XboxLive.signin offline_access" }
        };

        public static readonly Dictionary<string, string> UserAuthStateData = new()
        {
            { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
            { "client_id", ClientId },
            { "device_code", string.Empty }
        };

        public static readonly Dictionary<string, string> AuthTokenData = new()
        {
            { "client_id", ClientId },
            { "code", string.Empty },
            { "grant_type", "authorization_code" },
            { "redirect_uri", RedirectUri.ToString() },
            { "scope", "XboxLive.signin offline_access" }
        };

        public static readonly Dictionary<string, string> RefreshTokenData = new()
        {
            { "client_id", ClientId },
            { "client_secret", ClientSecret },
            { "refresh_token", string.Empty },
            { "grant_type", "refresh_token" },
            { "scope", "XboxLive.signin offline_access" }
        };
    }

    public static class RequireData
    {
        public record XboxLiveAuthRequire
        {
            public record PropertiesData
            {
                [JsonPropertyName("AuthMethod")] public static string AuthMethod = "RPS";
                [JsonPropertyName("SiteName")] public static string SiteName = "user.auth.xboxlive.com";
                [JsonPropertyName("RpsTicket")] public required string RpsTicket { get; set; }
            }

            [JsonPropertyName("PropertiesData")] public PropertiesData Properties { get; set; }
            [JsonPropertyName("RelyingParty")] public static string RelyingParty => "http://auth.xboxlive.com";
            [JsonPropertyName("TokenType")] public static string TokenType = "JWT";
        }

        public record XstsRequire
        {
            public record PropertiesData
            {
                public static string SandboxId = "RETAIL";
                public required List<string> UserTokens { get; set; }
            }

            public PropertiesData Properties { get; set; }
            public static string RelyingParty = "rp://api.minecraftservices.com/";
            public static string TokenType = "JWT";
        }

        public class MiecraftAccessTokenRequire
        {
            [JsonPropertyName("identityToken")] public string IdentityToken { get; set; }
        }
    }

    public static class ResponseData
    {
        public record AccessTokenResponce
        {
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("scope")] public string Scopr { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
            [JsonPropertyName("ext_expires_in")] public int ExtExpiresIn { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
        }

        public record DeviceCodeResponce(
            [property: JsonPropertyName("device_code")]
            string DeviceCode,
            [property: JsonPropertyName("user_code")]
            string UserCode,
            [property: JsonPropertyName("verification_uri")]
            string VerificationUri,
            [property: JsonPropertyName("expires_in")]
            int ExpiresIn,
            [property: JsonPropertyName("interval")]
            int Interval,
            [property: JsonPropertyName("message")]
            string Message
        );

        public record UserAuthStateResponse(
            [property: JsonPropertyName("token_type")]
            string TokenType,
            [property: JsonPropertyName("scope")] string Scope,
            [property: JsonPropertyName("expires_in")]
            int ExpiresIn,
            [property: JsonPropertyName("access_token")]
            string AccessToken,
            [property: JsonPropertyName("refresh_token")]
            string RefreshToken,
            [property: JsonPropertyName("error")] string Error,
            [property: JsonPropertyName("error_description")]
            string ErrorDescription,
            [property: JsonPropertyName("correlation_id")]
            string CorrelationId
        );

        public record XboxResponce
        {
            public record DisplayClainsData
            {
                public record XuiData
                {
                    [JsonPropertyName("uhs")] public string Uhs { get; set; }
                }

                [JsonPropertyName("xui")] public List<XuiData> Xui { get; set; }
            }

            [JsonPropertyName("IssueInstant")] public DateTime IssueInstant { get; set; }
            [JsonPropertyName("NotAfter")] public DateTime NotAfter { get; set; }
            [JsonPropertyName("Token")] public string Token { get; set; }
            [JsonPropertyName("DisplayClaims")] public DisplayClainsData DisplayClains { get; set; }
        }

        public record MinecraftAccessTokenResponce
        {
            [JsonPropertyName("username")] public string Username { get; set; }
            [JsonPropertyName("roles")] public List<object> Roles { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        }

        public record CheckHaveGameResponce
        {
            public record Item
            {
                [JsonPropertyName("name")] public string Name { get; set; }
                [JsonPropertyName("signature")] public string Signature { get; set; }
            }

            [JsonPropertyName("items")] public List<Item> Items { get; set; }
            [JsonPropertyName("signature")] public string Signature { get; set; }
            [JsonPropertyName("keyId")] public string KeyId { get; set; }
        }

        public record MinecraftPlayerUuidResponse
        {
            public record Skin
            {
                [JsonPropertyName("id")] public string Id { get; set; }

                [JsonPropertyName("state")] public string State { get; set; }

                [JsonPropertyName("url")] public string Url { get; set; }

                [JsonPropertyName("variant")] public string Variant { get; set; }

                [JsonPropertyName("textureKey")] public string TextureKey { get; set; }

                [JsonPropertyName("alias")] public string Alias { get; set; }
            }

            public record Cape(
                [property: JsonPropertyName("id")] string Id,
                [property: JsonPropertyName("state")] string State,
                [property: JsonPropertyName("url")] string Url,
                [property: JsonPropertyName("alias")] string Alias
            );

            [JsonPropertyName("id")] public string Uuid { get; set; }

            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("skins")] public List<Skin> Skins { get; set; }

            [JsonPropertyName("capes")] public List<Cape> Capes { get; set; }
        }
    }
}