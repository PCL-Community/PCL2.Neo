using PCL2.Neo.Models.Account.OAuthService.RedirectServer;
using PCL2.Neo.Utils;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.OAuthService;

public class AuthCodeMode
{
    public static async Task<AccountInfo> LogIn()
    {
        try
        {
            var authCode = GetAuthCode();
            var authToken = await GetAuthToken(authCode);
            var minecraftAccessToken = await OAuth.GetMinecraftToken(authToken.AccessToken);

            var playerUuidAndName = await MinecraftInfo.GetPlayerUuid(minecraftAccessToken);

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
                Capes = MinecraftInfo.CollectCapes(playerUuidAndName.Capes),
                Skins = MinecraftInfo.CollectSkins(playerUuidAndName.Skins)
            };
        }
        catch (Exception)
        {
            throw;
            // todo: log this exception
        }
    }

    private static string GetAuthCode()
    {
        var url = OAuthData.FormUrlReqData.AuthCodeData;
        var redirectServer = new RedirectServer.RedirectServer(5050); // todo: set prot in app configureation
        var authCode = new AuthCode();
        redirectServer.Subscribe(authCode);

        // todo: this code will unuseable in some system. we need handle this error
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // todo: time out handle

        return authCode.GetAuthCode().Code;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(OAuthData.FormUrlReqData))]
    public static async ValueTask<OAuthData.ResponceData.AccessTokenResponce> GetAuthToken(string authCode)
    {
        var authTokenData = OAuthData.FormUrlReqData.AuthTokenData;
        authTokenData["authCode"] = authCode;

        return await Net.SendHttpRequestAsync<OAuthData.ResponceData.AccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri,
            new FormUrlEncodedContent(authTokenData));
    }
}