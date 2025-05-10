using PCL.Neo.Core.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PCL.Neo.Core.Models.Account.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency

public static class OAuth
{
    public static async Task<OAuthData.ResponseData.AccessTokenResponce> RefreshToken(string refreshToken)
    {
        var authTokenData = OAuthData.FormUrlReqData.RefreshTokenData;
        authTokenData["refresh_token"] = refreshToken;

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.AccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri,
            new FormUrlEncodedContent(authTokenData));
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async ValueTask<OAuthData.ResponseData.XboxResponce> GetXboxToken(string accessToken)
    {
        var jsonContent = new OAuthData.RequireData.XboxLiveAuthRequire
        {
            Properties = new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData { RpsTicket = accessToken }
        };

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponce>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XboxLiveAuth,
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

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponce>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XstsAuth,
            jsonContent,
            JsonSerializerOptions.Web);

        return response.Token;
    }

    public static async Task<string> GetMinecraftToken(string accessToken)
    {
        var xboxToken = await GetXboxToken(accessToken);
        var xstsToken = await GetXstsToken(xboxToken.Token);
        var minecraftAccessToken =
            await MinecraftInfo.GetMinecraftAccessToken(xboxToken.DisplayClains.Xui.First().Uhs, xstsToken);

        if (!await MinecraftInfo.HaveGame(minecraftAccessToken))
        {
            throw new MinecraftInfo.NotHaveGameException("Logged-in user does not own any game!");
        }

        return minecraftAccessToken;
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion
}