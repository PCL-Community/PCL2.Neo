using PCL2.Neo.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.OAuthService;
#pragma warning disable IL2026 // fixed by DynamicDependency
public class MinecraftInfo
{
    public class NotHaveGameException(string msg) : Exception(msg);

    public static List<AccountInfo.Skin> CollectSkins(
        IEnumerable<OAuthData.ResponceData.MinecraftPlayerUuidResponse.Skin> skins) =>
        skins.Select(skin => new
            {
                skin,
                state = skin.State switch
                {
                    "ACTIVE" => AccountInfo.State.Active,
                    "INACTIVE" => AccountInfo.State.Inactive,
                    _ => throw new ArgumentOutOfRangeException()
                },
                url = new Uri(skin.Url)
            })
            .Select(t =>
                new AccountInfo.Skin(t.skin.Id, t.url, t.skin.Variant, t.skin.TextureKey,
                    t.state))
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
            },
            url = new Uri(cape.Url)
        }))
        .Select(t =>
            new AccountInfo.Cape(t.cape.Id, t.state, t.url, t.cape.Alias))
        .ToList();

    public static async ValueTask<string> GetMinecraftAccessToken(string uhs, string xstsToken)
    {
        var jsonContent = new OAuthData.RequireData.MiecraftAccessTokenRequire
        {
            IdentityToken = $"XBL3.0 x={uhs};{xstsToken}"
        };

        var response = await Net.SendHttpRequestAsync<OAuthData.ResponceData.MinecraftAccessTokenResponce>(
            HttpMethod.Post,
            OAuthData.RequestUrls.MinecraftAccessTokenUri,
            jsonContent,
            JsonSerializerOptions.Web);

        return response.AccessToken;
    }

    public static async ValueTask<bool> HaveGame(string accessToken)
    {
        var response = await Net.SendHttpRequestAsync<OAuthData.ResponceData.CheckHaveGameResponce>(
            HttpMethod.Get,
            OAuthData.RequestUrls.CheckHasMc,
            bearerToken: accessToken);

        return response.Items.Any(it => !string.IsNullOrEmpty(it.Signature));
    }

    public static async ValueTask<OAuthData.ResponceData.MinecraftPlayerUuidResponse>
        GetPlayerUuid(string accessToken) =>
        await Net.SendHttpRequestAsync<OAuthData.ResponceData.MinecraftPlayerUuidResponse>(
            HttpMethod.Get,
            OAuthData.RequestUrls.PlayerUuidUri,
            bearerToken: accessToken);
}