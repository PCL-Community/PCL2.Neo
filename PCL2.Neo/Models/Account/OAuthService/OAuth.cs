using PCL2.Neo.Models.Account.OAuthService.RedirectServer;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency
public static class OAuth
{
    // todo: add device code auth
    // todo: optimize dulicate codes
    public static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static async void LogIn()
    {
        try
        {
            var authCode = GetAuthCode();
            var authToken = await GetAuthToken(authCode);
            var xblToken = await GetRefreshToken(authToken);
            var xstsToken = await GetXstsToken(xblToken.Token);
            var minecraftAccessToken = await GetMinecraftAccessToken(xblToken.Uhs, xstsToken);
            var haveGame = await HaveGame(minecraftAccessToken);
            var playerUuidAndName = await GetPlayerUuidAndName(minecraftAccessToken);
            // todo: storage this token, uuid, name, etc.
        }
        catch (Exception)
        {
            // todo: log this exception
        }
    }

    private static int GetAuthCode()
    {
        var url = OAuthData.AuthReqData.AuthCodeData;
        var redirectServer = new RedirectServer.RedirectServer(5050); // todo: set prot in app configureation
        var authCode = new AuthCode();
        redirectServer.Subscribe(authCode);

        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // todo: time out handle

        return authCode.GetAuthCode().Code;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthReqData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    public static async ValueTask<string> GetAuthToken(int authCode)
    {
        var authTokenData = OAuthData.AuthReqData.AuthTokenData;
        authTokenData["authCode"] = authCode.ToString();
        var content = new FormUrlEncodedContent(authTokenData);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        using var response = await HttpClient.PostAsync(OAuthData.AuthUrls.AuthTokenUri, content);

        response.EnsureSuccessStatusCode();

        var authToken = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.AuthCodeResponce>();

        ArgumentNullException.ThrowIfNull(authToken);

        return authToken.AccessToken;
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.AuthUrls))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.ResponceData))]
    public static async ValueTask<XblToken> GetRefreshToken(string accessToken)
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