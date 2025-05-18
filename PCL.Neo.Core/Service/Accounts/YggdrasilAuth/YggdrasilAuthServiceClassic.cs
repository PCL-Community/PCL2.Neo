using PCL.Neo.Core.Utils;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Accounts.YggdrasilAuth;

public class YggdrasilAuthServiceClassic : IYggdrasilAuthServiceClassic
{
    /// <inheritdoc />
    public Uri BaseUrl { get; private set; }

    /// <inheritdoc />
    public string ClientToken { get; set; }

    /// <inheritdoc />
    public string CurrentAccessToken { get; set; }

    /// <inheritdoc />
    public YggdrasilClassicData.SelectedProfileData? CurrentProfileData { get; set; }

    /// <inheritdoc />
    public async Task<Result<Uri, Exception>> GetAliAsync(Uri url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        try
        {
            var response = await Net.SharedHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.Headers.TryGetValues("X-Authlib-Injector-API-Location", out IEnumerable<string>? values))
            {
                var ali = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(ali))
                {
                    BaseUrl = new Uri(ali);

                    return Result<Uri, Exception>.Ok(new Uri(ali));
                }
            }

            return Result<Uri, Exception>.Ok(url);
        }
        catch (Exception e)
        {
            return Result<Uri, Exception>.Fail(e);
        }
    }

    /// <inheritdoc />
    public async Task<Result<YggdrasilClassicData.Response.MetaInfo, Exception>> GetAuthServerMetaInfoAsync()
    {
        var meteUri = new Uri(BaseUrl, "meta");
        try
        {
            var result =
                await Net.SendHttpRequestAsync<YggdrasilClassicData.Response.MetaInfo>(HttpMethod.Get, meteUri);
            return Result<YggdrasilClassicData.Response.MetaInfo, Exception>.Ok(result);
        }
        catch (Exception e)
        {
            return Result<YggdrasilClassicData.Response.MetaInfo, Exception>.Fail(e);
        }
    }

    /// <inheritdoc />
    public async Task<Result<YggdrasilClassicData.Response.Login, Exception>> LoginAsync(string email, string pwd)
    {
        var content = new YggdrasilClassicData.Request.Login
        {
            Agent = new YggdrasilClassicData.Request.Login.AgentData { Name = "Minecraft", Version = 1 },
            UserName = email,
            Password = pwd,
            Requestuser = false
        };

        var requestUri = new Uri(BaseUrl, "authserver/authenticate");

        try
        {
            var result =
                await Net.SendHttpRequestAsync<YggdrasilClassicData.Response.Login>(HttpMethod.Post, requestUri,
                    content);

            if (result.SelectedProfile is not null)
            {
                CurrentProfileData = result.SelectedProfile;
            }

            CurrentAccessToken = result.AccessToken;
            ClientToken = result.ClientToken;

            return Result<YggdrasilClassicData.Response.Login, Exception>.Ok(result);
        }
        catch (Exception e)
        {
            return Result<YggdrasilClassicData.Response.Login, Exception>.Fail(e);
        }
    }

    /// <inheritdoc />
    public async Task<Result<YggdrasilClassicData.Response.Refresh, Exception>> RefreshAsync(
        YggdrasilClassicData.SelectedProfileData? selectedProfile = null)
    {
        var content = new YggdrasilClassicData.Request.Refresh
        {
            AccessToken = CurrentAccessToken,
            ClientToken = ClientToken,
            RequestUser = false,
            SelectedProfile = CurrentProfileData
        };
        var requestUri = new Uri(BaseUrl, "authserver/refresh");
        try
        {
            var result =
                await Net.SendHttpRequestAsync<YggdrasilClassicData.Response.Refresh>(HttpMethod.Post,
                    requestUri,
                    content);

            CurrentAccessToken = result.AccessToken;
            ClientToken = result.ClientToken;
            return Result<YggdrasilClassicData.Response.Refresh, Exception>.Ok(result);
        }
        catch (Exception e)
        {
            return Result<YggdrasilClassicData.Response.Refresh, Exception>.Fail(e);
        }
    }

    /// <inheritdoc />
    public async Task<Result<bool, Exception>> Validata()
    {
        var jsonInfo = new YggdrasilClassicData.Request.Validata
        {
            AccessToken = CurrentAccessToken, ClientToken = ClientToken
        };
        var requestUri = new Uri(BaseUrl, "authserver/validate");
        var content = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(jsonInfo) };
        var response = await Net.SharedHttpClient.SendAsync(content);
        return response.StatusCode == HttpStatusCode.NoContent
            ? Result<bool, Exception>.Ok(true)
            : Result<bool, Exception>.Fail(new Exception("Validation failed."));
    }

    /// <inheritdoc />
    public async Task Invalidata()
    {
        var jsonInfo = new YggdrasilClassicData.Request.Invalidata()
        {
            AccessToken = CurrentAccessToken, ClientToken = ClientToken
        };
        var requestUri = new Uri(BaseUrl, "authserver/invalidate");
        var content = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(jsonInfo) };
        await Net.SharedHttpClient.SendAsync(content);

        CurrentAccessToken = string.Empty;
    }

    /// <inheritdoc />
    public async Task SignoutAsync(string userName, string pwd)
    {
        var jsonInfo   = new YggdrasilClassicData.Request.Signout() { UserName = userName, Password = pwd };
        var requestUri = new Uri(BaseUrl, "authserver/signout");
        var content    = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = JsonContent.Create(jsonInfo) };
        await Net.SharedHttpClient.SendAsync(content);
        // #TODO 这里没有实现登录失败的情况，请后人帮忙了

        CurrentAccessToken = string.Empty;
        CurrentProfileData = null;
        ClientToken        = string.Empty;
    }

    /// <inheritdoc />
    public async Task<Result<YggdrasilClassicData.PlayerTexture, Exception>> GetPlayerTextureAsync()
    {
        var requestUri = new Uri(BaseUrl, $"sessionserver/session/prifile/{CurrentProfileData.Id}?unsigned=true");
        var response   = await Net.SendHttpRequestAsync<YggdrasilClassicData.PlayerProfile>(HttpMethod.Get, requestUri);
        var textureValue = response.Properties
            .Where(it => it.Name == "textures")
            .Select(it => it.Value)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(textureValue))
        {
            return Result<YggdrasilClassicData.PlayerTexture, Exception>.Fail(
                new ArgumentException("Player Texture not found."));
        }

        var textureStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(textureValue));
        var jsonRecord = JsonSerializer.Deserialize<YggdrasilClassicData.PlayerTexture>(textureStr);

        if (jsonRecord == null)
        {
            return Result<YggdrasilClassicData.PlayerTexture, Exception>.Fail(
                new ArgumentNullException(nameof(jsonRecord)));
        }

        return Result<YggdrasilClassicData.PlayerTexture, Exception>.Ok(jsonRecord);
    }
}