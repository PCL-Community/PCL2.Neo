using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL2.Neo.Models.Account.OAuthService;

public static class OAuthData
{
#nullable disable
    public static class AuthUrls
    {
        public static readonly Uri AuthCodeUri =
            new("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize");

        public static readonly Uri AuthTokenUri = new("https://login.microsoftonline.com/consumers/oauth2/v2.0/token");
        public static readonly Uri XboxLiveAuth = new("https://user.auth.xboxlive.com/user/authenticate");
        public static readonly Uri XstsAuth = new("https://xsts.auth.xboxlive.com/xsts/authorize");

        public static readonly Uri McAccessTokenUri =
            new("https://api.minecraftservices.com/authentication/login_with_xbox");

        public static readonly Uri CheckHasMc = new("https://api.minecraftservices.com/entitlements/mcstore");
        public static readonly Uri PlayerUuidUri = new("https://api.minecraftservices.com/minecraft/profile");
    }

    public static class AuthReqData
    {
        // todo: optimize this code with class for safe
        public static readonly string ClientId = string.Empty;
        public static readonly Uri RedirectUri = new("http://127.0.0.1:5050"); // TODO: Updata Uri
        public static readonly string ClientSecret = string.Empty; // TODO: Set client secret

        public static string AuthCodeData
        {
            get => new(
                $"{AuthUrls.AuthCodeUri}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&response_mode=query&scope=XboxLive.signin offline_access");
        }

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
        public class XboxLiveAuthRequire
        {
            public class PropertiesData
            {
                [JsonPropertyName("AuthMethod")] public string AuthMethod { get; } = "RPS";
                [JsonPropertyName("SiteName")] public string SiteName { get; } = "user.auth.xboxlive.com";
                [JsonPropertyName("RpsTicket")] public required string RpsTicket { get; set; }
            }

            [JsonPropertyName("PropertiesData")] public PropertiesData Properties { get; set; }
            [JsonPropertyName("RelyingParty")] public string RelyingParty { get; } = "http://auth.xboxlive.com";
            [JsonPropertyName("TokenType")] public string TokenType { get; } = "JWT";
        }

        public class XstsRequire
        {
            public class PropertiesData
            {
                public string SandboxId { get; } = "RETAIL";
                public required List<string> UserTokens { get; set; }
            }

            public PropertiesData Properties { get; set; }
            public string RelyingParty { get; } = "rp://api.minecraftservices.com/";
            public string TokenType { get; } = "JWT";
        }

        public class MiecraftAccessTokenRequire
        {
            [JsonPropertyName("identityToken")] public string IdentityToken { get; set; }
        }
    }

    public static class ResponceData
    {
        public class AuthCodeResponce
        {
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("scope")] public string Scopr { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
            [JsonPropertyName("ext_expires_in")] public int ExtExpiresIn { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
        }

        public class XboxResponce
        {
            public class DisplayClainsData
            {
                public class XuiData
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

        public class MinecraftAccessTokenResponce
        {
            [JsonPropertyName("username")] public string Username { get; set; }
            [JsonPropertyName("roles")] public List<object> Roles { get; set; }
            [JsonPropertyName("access_token")] public string AccessToken { get; set; }
            [JsonPropertyName("token_type")] public string TokenType { get; set; }
            [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        }

        public class CheckHaveGameResponce
        {
            public class Item
            {
                [JsonPropertyName("name")] public string Name { get; set; }
                [JsonPropertyName("signature")] public string Signature { get; set; }
            }

            [JsonPropertyName("items")] public List<Item> Items { get; set; }
            [JsonPropertyName("signature")] public string Signature { get; set; }
            [JsonPropertyName("keyId")] public string KeyId { get; set; }
        }

        public class MinecraftPlayerUuidResponse
        {
            public class Skin
            {
                [JsonPropertyName("id")] public string Id { get; set; }

                [JsonPropertyName("state")] public string State { get; set; }

                [JsonPropertyName("url")] public string Url { get; set; }

                [JsonPropertyName("variant")] public string Variant { get; set; }

                [JsonPropertyName("alias")] public string Alias { get; set; }
            }

            [JsonPropertyName("id")] public string Id { get; set; }

            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("skins")] public List<Skin> Skins { get; set; }

            [JsonPropertyName("capes")] public List<object> Capes { get; set; } // todo: set type of capes
        }
    }
#nullable enable
}