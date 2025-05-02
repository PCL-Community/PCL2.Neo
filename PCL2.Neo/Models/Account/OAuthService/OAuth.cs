using PCL2.Neo.Models.Account.OAuthService.RedirectServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency
public static class OAuth
{
    public static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    #region NotHaveGameException

    public class NotHaveGameException : Exception
    {
        /// <inheritdoc />
        public NotHaveGameException()
        {
        }

        /// <inheritdoc />
        [Obsolete("Obsolete")]
        protected NotHaveGameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc />
        public NotHaveGameException(string? message) : base(message)
        {
        }

        /// <inheritdoc />
        public NotHaveGameException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    #endregion

    public static async Task<AccountInfo> LogIn()
    {
        try
        {
            var authCode = GetAuthCode();
            var authToken = await GetAuthToken(authCode);
            var minecraftAccessToken = await GetMinecraftToken(authToken.AccessToken);

            var playerUuidAndName = await GetPlayerUuid(minecraftAccessToken);

            return new AccountInfo
            {
                AccessToken = minecraftAccessToken,
                OAuthToken =
                    new AccountInfo.OAuthTokenData(authToken.AccessToken, authToken.RefreshToken,
                        new DateTimeOffset(DateTime.Today, TimeSpan.FromSeconds(authToken.ExpiresIn))),
                UserName = playerUuidAndName.Name,
                UserProperties = string.Empty,
                UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
                Uuid = playerUuidAndName.Uuid,
                Capes = CollectCapes(playerUuidAndName.Capes),
                Skins = CollectSkins(playerUuidAndName.Skins)
            };
        }
        catch (Exception e)
        {
            throw;
            // todo: log this exception
        }
    }

    public static List<AccountInfo.Skin> CollectSkins(
        IEnumerable<OAuthData.ResponceData.MinecraftPlayerUuidResponse.Skin> skins) =>
        (skins.Select(skin => new
            {
                skin,
                state = skin.State switch
                {
                    "ACTIVE" => AccountInfo.State.Active,
                    "INACTIVE" => AccountInfo.State.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                }
            })
            .Select(t => new { t, url = new Uri(t.skin.Url) })
            .Select(t =>
                new AccountInfo.Skin(t.t.skin.Id, t.url, t.t.skin.Variant, t.t.skin.TextureKey,
                    t.t.state)))
        .ToList();

    public static List<AccountInfo.Cape> CollectCapes(
        IEnumerable<OAuthData.ResponceData.MinecraftPlayerUuidResponse.Cape> capes) =>
        (capes.Select(cape => new
        {
            cape,
            state = cape.State switch
            {
                "ACTIVE" => AccountInfo.State.Active,
                "INACTIVE" => AccountInfo.State.Inactive,
                _ => throw new ArgumentOutOfRangeException()
            }
        }))
        .Select(t => new { t, url = new Uri(t.cape.Url) })
        .Select(t =>
            new AccountInfo.Cape(t.t.cape.Id, t.t.state, t.url, t.t.cape.Alias))
        .ToList();

    public static async Task<string> GetMinecraftToken(string accessToken)
    {
        var xboxToken = await GetXboxToken(accessToken);
        var xstsToken = await GetXstsToken(xboxToken.Token);
        var minecraftAccessToken = await GetMinecraftAccessToken(xboxToken.Uhs, xstsToken);

        if (!await HaveGame(minecraftAccessToken))
        {
            throw new NotHaveGameException("Logged-in user does not own any game!");
        }

        return minecraftAccessToken;
    }


    #region AuthAndRefreshToken

    public record AuthAndRefreshToken(string AccessToken, string RefreshToken);

    #endregion

    // 通用的 HTTP 请求方法
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async Task<TResponse> SendHttpRequestAsync<TResponse>(
        HttpMethod method,
        Uri url,
        object? content = null,
        JsonSerializerOptions? jsonOptions = null,
        string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(method, url);

        // 设置请求体
        if (content != null)
        {
            if (content is FormUrlEncodedContent formContent)
            {
                request.Content = formContent;
            }
            else
            {
                request.Content = JsonContent.Create(content, options: jsonOptions);
            }
        }

        // 设置授权头
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        // 发送请求
        using var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // 解析响应
        var result = await response.Content.ReadFromJsonAsync<TResponse>(jsonOptions);
        ArgumentNullException.ThrowIfNull(result);

        return result;
    }

    public static async Task<OAuthData.ResponceData.AccessTokenResponce> RefreshToken(string refreshToken)
    {
        var authTokenData = OAuthData.EUrlReqData.RefreshTokenData;
        authTokenData["refresh_token"] = refreshToken;

        return await SendHttpRequestAsync<OAuthData.ResponceData.AccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.AuthUrls.AuthTokenUri,
            new FormUrlEncodedContent(authTokenData));
    }

    private static string GetAuthCode()
    {
        var url = OAuthData.EUrlReqData.AuthCodeData;
        var redirectServer = new RedirectServer.RedirectServer(5050); // todo: set prot in app configureation
        var authCode = new AuthCode();
        redirectServer.Subscribe(authCode);

        // todo: this code will unuseable in some system. we need handle this error
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // todo: time out handle

        return authCode.GetAuthCode().Code;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.EUrlReqData))]
    public static async ValueTask<OAuthData.ResponceData.AccessTokenResponce> GetAuthToken(string authCode)
    {
        var authTokenData = OAuthData.EUrlReqData.AuthTokenData;
        authTokenData["authCode"] = authCode;

        return await SendHttpRequestAsync<OAuthData.ResponceData.AccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.AuthUrls.AuthTokenUri,
            new FormUrlEncodedContent(authTokenData));
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async ValueTask<XblToken> GetXboxToken(string accessToken)
    {
        var jsonContent = new OAuthData.RequireData.XboxLiveAuthRequire
        {
            Properties = new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData { RpsTicket = accessToken }
        };

        return await SendHttpRequestAsync<XblToken>(
            HttpMethod.Post,
            OAuthData.AuthUrls.XboxLiveAuth,
            jsonContent,
            JsonSerializerOptions.Web);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async ValueTask<string> GetXstsToken(string xblToken)
    {
        var jsonContent = new OAuthData.RequireData.XstsRequire
        {
            Properties = new OAuthData.RequireData.XstsRequire.PropertiesData { UserTokens = [xblToken] }
        };

        var response = await SendHttpRequestAsync<OAuthData.ResponceData.XboxResponce>(
            HttpMethod.Post,
            OAuthData.AuthUrls.XstsAuth,
            jsonContent,
            JsonSerializerOptions.Web);

        return response.Token;
    }

    public static async ValueTask<string> GetMinecraftAccessToken(string uhs, string xstsToken)
    {
        var jsonContent = new OAuthData.RequireData.MiecraftAccessTokenRequire
        {
            IdentityToken = $"XBL3.0 x={uhs};{xstsToken}"
        };

        var response = await SendHttpRequestAsync<OAuthData.ResponceData.MinecraftAccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.AuthUrls.McAccessTokenUri,
            jsonContent,
            JsonSerializerOptions.Web);

        return response.AccessToken;
    }

    public static async ValueTask<bool> HaveGame(string accessToken)
    {
        var response = await SendHttpRequestAsync<OAuthData.ResponceData.CheckHaveGameResponce>(
            HttpMethod.Get,
            OAuthData.AuthUrls.CheckHasMc,
            bearerToken: accessToken);

        return response.Items.Any(it => !string.IsNullOrEmpty(it.Signature));
    }

    public static async ValueTask<OAuthData.ResponceData.MinecraftPlayerUuidResponse>
        GetPlayerUuid(string accessToken) =>
        await SendHttpRequestAsync<OAuthData.ResponceData.MinecraftPlayerUuidResponse>(
            HttpMethod.Get,
            OAuthData.AuthUrls.PlayerUuidUri,
            bearerToken: accessToken);
}