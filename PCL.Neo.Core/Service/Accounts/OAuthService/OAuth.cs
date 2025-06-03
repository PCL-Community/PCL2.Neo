using PCL.Neo.Core.Service.Accounts.OAuthService.Exceptions;
using PCL.Neo.Core.Utils;
using System.Diagnostics.CodeAnalysis;

namespace PCL.Neo.Core.Service.Accounts.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency

public static class OAuth
{
    public static async Task<OAuthData.ResponseData.AccessTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var authTokenData = OAuthData.FormUrlReqData.RefreshTokenData.Value.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value
        );
        authTokenData["refresh_token"] = refreshToken;

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.AccessTokenResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri.Value,
            new FormUrlEncodedContent(authTokenData));
    }

    [DynamicDependency("PublicConstructors", typeof(OAuthData.RequireData))]
    public static async Task<OAuthData.ResponseData.XboxResponse> GetXboxTokenAsync(string accessToken)
    {
        var jsonContent =
            new OAuthData.RequireData.XboxLiveAuthRequire
            {
                Properties = new OAuthData.RequireData.XboxLiveAuthRequire.PropertiesData(accessToken)
            };

        return await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XboxLiveAuth.Value,
            jsonContent);
    }

    [DynamicDependency("PublicConstructors", typeof(OAuthData.RequireData))]
    public static async Task<string> GetXstsTokenAsync(string xblToken)
    {
        List<string> tokens = new List<string> { xblToken };
        var jsonContent =
            new OAuthData.RequireData.XstsRequire(new OAuthData.RequireData.XstsRequire.PropertiesData(tokens));

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponseData.XboxResponse>(
            HttpMethod.Post,
            OAuthData.RequestUrls.XstsAuth.Value,
            jsonContent);

        return response.Token;
    }

    public static async Task<string> GetMinecraftTokenAsync(string accessToken)
    {
        var xboxToken = await GetXboxTokenAsync(accessToken);
        var xstsToken = await GetXstsTokenAsync(xboxToken.Token);
        var minecraftAccessToken =
            await MinecraftInfo.GetMinecraftAccessTokenAsync(xboxToken.DisplayClaims.Xui.First().Uhs, xstsToken);

        if (!await MinecraftInfo.IsHaveGameAsync(minecraftAccessToken))
        {
            throw new NotHaveGameException("Logged-in user does not own any game!");
        }

        return minecraftAccessToken;
    }

    #region XblToken

    public record XblToken(string Token, string Uhs);

    #endregion
}