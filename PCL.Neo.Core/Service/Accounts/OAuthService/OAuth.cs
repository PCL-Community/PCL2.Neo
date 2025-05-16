using PCL.Neo.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency

public static class OAuth
{
    public static async Task<OAuthData.ResponseData.AccessTokenResponse> RefreshToken(string refreshToken)
    {
        var authTokenData = new Dictionary<string, string>(OAuthData.FormUrlReqData.RefreshTokenData)
        {
            ["refresh_token"] = refreshToken
        };

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.AccessTokenResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri,
            new FormUrlEncodedContent(authTokenData));
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<OAuthData.ResponseData.XboxResponse> GetXboxToken(string accessToken)
    {
        var jsonContent =
            new OAuthData.RequireData.XboxLiveAuthRequire(
                new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData(accessToken));

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XboxLiveAuth,
            jsonContent);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.RequireData))]
    public static async Task<string> GetXstsToken(string xblToken)
    {
        var jsonContent =
            new OAuthData.RequireData.XstsRequire(new OAuthData.RequireData.XstsRequire.PropertiesData([xblToken]));

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XstsAuth,
            jsonContent);

        return response.Token;
    }

    public static async Task<string> GetMinecraftToken(string accessToken)
    {
        var xboxToken = await GetXboxToken(accessToken);
        var xstsToken = await GetXstsToken(xboxToken.Token);
        var minecraftAccessToken =
            await MinecraftInfo.GetMinecraftAccessToken(xboxToken.DisplayClaims.Xui.First().Uhs, xstsToken);

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