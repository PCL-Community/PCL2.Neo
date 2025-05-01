using PCL2.Neo.Models.Account.OAuthService.RedirectServer;
using System;
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
    // todo: add device code auth ( in microsoftAuth.cs )
    // todo: optimize dulicate codes
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
            var xboxToken = await GetXboxToken(authToken.AccessToken);
            var xstsToken = await GetXstsToken(xboxToken.Token);
            var minecraftAccessToken = await GetMinecraftAccessToken(xboxToken.Uhs, xstsToken);
            var haveGame = await HaveGame(minecraftAccessToken);

            if (!haveGame)
            {
                throw new NotHaveGameException("Logined user not have any game!");
            }

            var playerUuidAndName = await GetPlayerUuidAndName(minecraftAccessToken);

            return new AccountInfo()
            {
                AccessToken = minecraftAccessToken,
                RefreshToken = authToken.RefreshToken,
                UserName = playerUuidAndName.Name,
                UserProperties = string.Empty,
                UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
                Uuid = playerUuidAndName.Uuid
            };
            // todo: storage this token, uuid, name, etc.
        }
        catch (Exception e)
        {
            throw;
            // todo: log this exception
        }
    }

    #region AuthAndRefreshToken

    public record AuthAndRefreshToken(string AccessToken, string RefreshToken);

    #endregion


    public static async Task<AuthAndRefreshToken> RefreshToken(string refreshToken)
    {
        try
        {
            var authTokenData = OAuthData.EUrlReqData.RefreshTokenData;
            authTokenData["refresh_token"] = refreshToken;
            var content = new FormUrlEncodedContent(authTokenData);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            using var response = await HttpClient.PostAsync(OAuthData.AuthUrls.AuthTokenUri, content);

            response.EnsureSuccessStatusCode();

            var authToken = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.AccessTokenResponce>();
            ArgumentNullException.ThrowIfNull(authToken);

            return new AuthAndRefreshToken(authToken.AccessToken, authToken.RefreshToken);
        }
        catch (Exception e)
        {
            throw;
            // todo: log this exception
        }
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
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    public static async ValueTask<AuthAndRefreshToken> GetAuthToken(string authCode)
    {
        var authTokenData = OAuthData.EUrlReqData.AuthTokenData;
        authTokenData["authCode"] = authCode;
        var content = new FormUrlEncodedContent(authTokenData);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        using var response = await HttpClient.PostAsync(OAuthData.AuthUrls.AuthTokenUri, content);

        response.EnsureSuccessStatusCode();

        var authToken = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.AccessTokenResponce>();
        ArgumentNullException.ThrowIfNull(authToken);

        return new AuthAndRefreshToken(authToken.AccessToken, authToken.RefreshToken);
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<XblToken> GetXboxToken(string accessToken)
    {
        var jsonContent = new OAuthData.RequireData.XboxLiveAuthRequire
        {
            Properties = new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData() { RpsTicket = accessToken }
        };

        using var response =
            await HttpClient.PostAsJsonAsync(OAuthData.AuthUrls.XboxLiveAuth, jsonContent, JsonSerializerOptions.Web);

        response.EnsureSuccessStatusCode();

        var xboxContent = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.XboxResponce>();
        ArgumentNullException.ThrowIfNull(xboxContent);

        var token = xboxContent.Token;
        var uhs = xboxContent.DisplayClains.Xui.First().Uhs;

        return new XblToken(token, uhs);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<string> GetXstsToken(string xblToken)
    {
        var jsonContent = new OAuthData.RequireData.XstsRequire
        {
            Properties = new OAuthData.RequireData.XstsRequire.PropertiesData { UserTokens = [xblToken] }
        };

        using var response =
            await HttpClient.PostAsJsonAsync(OAuthData.AuthUrls.XstsAuth, jsonContent, JsonSerializerOptions.Web);

        response.EnsureSuccessStatusCode();

        var xstsContent = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.XboxResponce>();
        ArgumentNullException.ThrowIfNull(xstsContent);

        return xstsContent.Token;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<string> GetMinecraftAccessToken(string uhs, string xstsToken)
    {
        var jsonContent = new OAuthData.RequireData.MiecraftAccessTokenRequire
        {
            IdentityToken = $"XBL3.0 x={uhs};{xstsToken}"
        };
        using var response =
            await HttpClient.PostAsJsonAsync(OAuthData.AuthUrls.McAccessTokenUri, jsonContent,
                JsonSerializerOptions.Web);
        response.EnsureSuccessStatusCode();

        var minecraftContent =
            await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.MinecraftAccessTokenResponce>();
        ArgumentNullException.ThrowIfNull(minecraftContent);

        return minecraftContent.AccessToken;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<bool> HaveGame(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, OAuthData.AuthUrls.CheckHasMc);
        request.Headers.Authorization = new AuthenticationHeaderValue($"Bearer {accessToken}");

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var mcContent = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.CheckHaveGameResponce>();
        ArgumentNullException.ThrowIfNull(mcContent);

        var items = mcContent.Items;
        var haveGame = items.Any(it => it.Signature != string.Empty);

        return haveGame;
    }

    #region PlayerUuidAndName

    public record PlayerUuidAndName(string Uuid, string Name);

    #endregion

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<PlayerUuidAndName> GetPlayerUuidAndName(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, OAuthData.AuthUrls.PlayerUuidUri);
        request.Headers.Authorization = new AuthenticationHeaderValue($"Bearer {accessToken}");

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var mcContent = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.MinecraftPlayerUuidResponse>();
        ArgumentNullException.ThrowIfNull(mcContent);

        return new PlayerUuidAndName(mcContent.Id, mcContent.Name);
    }
}