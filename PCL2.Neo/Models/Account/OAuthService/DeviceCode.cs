using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace PCL2.Neo.Models.Account.OAuthService;
#pragma warning disable IL2026 // fixed by DynamicDependency
public static class DeviceCode
{
    public enum UserAuthMessage : byte
    {
        AuthorizationPending,
        BadVerificationCode,
        ExpiredToken
    }

    private static readonly ManualResetEvent IsUserAuthed = new ManualResetEvent(false);
    private static Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>? _useAuthResult;
    private static Timer? _timer;

    public record Result<TResult, TMessage>(bool IsSuccess, TMessage Message, TResult Res);

    public static async Task<AccountInfo> Login()
    {
        var codePair = await GetCodePair();
        _timer = new Timer(codePair.Interval);
        _timer.Elapsed += async (_, _) =>
        {
            if (await PollingServer(codePair))
            {
                IsUserAuthed.Set();
            }
        };
        _timer.Enabled = true;
        _timer.AutoReset = true;
        _timer.Start();

        IsUserAuthed.WaitOne();

        ArgumentNullException.ThrowIfNull(_useAuthResult);

        if (_useAuthResult.IsSuccess == false)
        {
            switch (_useAuthResult.Message) // todo: handle error
            {
                case UserAuthMessage.AuthorizationPending:
                    break;
                case UserAuthMessage.BadVerificationCode:
                    break;
                case UserAuthMessage.ExpiredToken:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        (string accessToken, string refreshToken) = (_useAuthResult.Res.AccessToken, _useAuthResult.Res.RefreshToken);
        var xboxToken = await OAuth.GetXboxToken(accessToken);
        var xstsToken = await OAuth.GetXstsToken(xboxToken.Token);
        var minecraftAccessToken = await OAuth.GetMinecraftAccessToken(xboxToken.Uhs, xstsToken);
        var haveGame = await OAuth.HaveGame(minecraftAccessToken);

        if (!haveGame)
        {
            throw new OAuth.NotHaveGameException("Logined user not have any game!");
        }

        var playerUuidAndName = await OAuth.GetPlayerUuidAndName(minecraftAccessToken);

        return new AccountInfo()
        {
            AccessToken = minecraftAccessToken,
            RefreshToken = refreshToken,
            UserName = playerUuidAndName.Name,
            UserProperties = string.Empty,
            UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
            Uuid = playerUuidAndName.Uuid
        };
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors,
        typeof(OAuthData.ResponceData.DeviceCodeResponce))]
    private static async Task<OAuthData.ResponceData.DeviceCodeResponce> GetCodePair()
    {
        var content = new FormUrlEncodedContent(OAuthData.EUrlReqData.DeviceCodeData);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        using var response = await OAuth.HttpClient.PostAsync(OAuthData.AuthUrls.DeviceCode, content);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.DeviceCodeResponce>();
        ArgumentNullException.ThrowIfNull(result);

        return result;
    }

    /// <summary>
    /// Stop timer and set IsUserAuthed to true. Only call this method when the polling is failed.
    /// </summary>
    private static void StopTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;

        IsUserAuthed.Set();
    }

    private static async Task<bool> PollingServer(OAuthData.ResponceData.DeviceCodeResponce pair)
    {
        var data = OAuthData.EUrlReqData.UserAuthStateData;
        data["device_code"] = pair.DeviceCode;
        var content = new FormUrlEncodedContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        using var response = await OAuth.HttpClient.PostAsync(OAuthData.AuthUrls.AuthTokenUri, content);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OAuthData.ResponceData.UserAuthStateResponce>();
        ArgumentNullException.ThrowIfNull(result);

        switch (result.Error)
        {
            case "authorization_pending":
                _useAuthResult =
                    new Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>(false,
                        UserAuthMessage.AuthorizationPending, result);
                StopTimer();
                return false;
            case "expired_token":
                _useAuthResult =
                    new Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>(false,
                        UserAuthMessage.ExpiredToken, result);
                StopTimer();
                return false;
            case "slow_down":
                _timer.Interval = _timer.Interval += 1000;
                break;
            case "bad_verification_code":
                _useAuthResult =
                    new Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>(false,
                        UserAuthMessage.BadVerificationCode, result);
                StopTimer();
                return false;
        }

        return true;
    }
}