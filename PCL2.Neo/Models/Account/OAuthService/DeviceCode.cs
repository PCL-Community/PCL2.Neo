using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace PCL2.Neo.Models.Account.OAuthService;

#pragma warning disable IL2026 // fixed by DynamicDependency
public static class DeviceCode
{
    public enum UserAuthMessage : byte
    {
        AuthorizationDeclined,
        BadVerificationCode,
        ExpiredToken,
        Success
    }

    public record Result<TResult, TMessage>(bool IsSuccess, TMessage Message, TResult Res);

    public class AuthorizationException(string message) : Exception(message);

    private static readonly ManualResetEvent IsUserAuthed = new(false);
    private static Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>? _useAuthResult;
    private static Timer? _timer;


    public static async Task<AccountInfo> Login()
    {
        var codePair = await GetCodePair();
        InitializeTimer(codePair.Interval, codePair.DeviceCode);

        IsUserAuthed.WaitOne();
        ValidateAuthResult();

        (string accessToken, string refreshToken) =
            (_useAuthResult!.Res.AccessToken, _useAuthResult.Res.RefreshToken);
        var minecraftAccessToken = await OAuth.GetMinecraftToken(accessToken);
        var playerUuidAndName = await OAuth.GetPlayerUuidAndName(minecraftAccessToken);

        return new AccountInfo
        {
            AccessToken = minecraftAccessToken,
            RefreshToken = refreshToken,
            UserName = playerUuidAndName.Name,
            UserProperties = string.Empty,
            UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
            Uuid = playerUuidAndName.Uuid
        };
    }

    private static void InitializeTimer(double interval, string deviceCode)
    {
        _timer = new Timer(interval) { AutoReset = true, Enabled = true };
        _timer.Elapsed += async (_, _) =>
        {
            if (await PollingServer(deviceCode))
            {
                IsUserAuthed.Set();
            }
        };
        _timer.Start();
    }

    private static void ValidateAuthResult()
    {
        ArgumentNullException.ThrowIfNull(_useAuthResult);

        if (_useAuthResult?.IsSuccess == false)
        {
            throw _useAuthResult.Message switch
            {
                UserAuthMessage.AuthorizationDeclined =>
                    new AuthorizationException("User was denied the authorization"),
                UserAuthMessage.BadVerificationCode => new AuthorizationException("Bad Verification Code"),
                UserAuthMessage.ExpiredToken => new AuthorizationException("Authorization time out"),
                _ => new ArgumentOutOfRangeException()
            };
        }
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors,
        typeof(OAuthData.ResponceData.DeviceCodeResponce))]
    private static async Task<OAuthData.ResponceData.DeviceCodeResponce> GetCodePair()
    {
        var content = new FormUrlEncodedContent(OAuthData.EUrlReqData.DeviceCodeData)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        return await OAuth.SendHttpRequestAsync<OAuthData.ResponceData.DeviceCodeResponce>(HttpMethod.Post,
            OAuthData.AuthUrls.DeviceCode, content, JsonSerializerOptions.Web);
    }

    private static async Task<bool> PollingServer(string deviceCode)
    {
        var data = OAuthData.EUrlReqData.UserAuthStateData;
        data["device_code"] = deviceCode;

        var content = new FormUrlEncodedContent(data)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var result = await OAuth.SendHttpRequestAsync<OAuthData.ResponceData.UserAuthStateResponce>(HttpMethod.Post,
            OAuthData.AuthUrls.AuthTokenUri, content, JsonSerializerOptions.Web);

        return HandlePollingResult(result);
    }

    private static bool HandlePollingResult(OAuthData.ResponceData.UserAuthStateResponce result)
    {
        _useAuthResult = result.Error switch
        {
            "authorization_declined" => CreateAuthResult(false, UserAuthMessage.AuthorizationDeclined, result),
            "expired_token" => CreateAuthResult(false, UserAuthMessage.ExpiredToken, result),
            "bad_verification_code" => CreateAuthResult(false, UserAuthMessage.BadVerificationCode, result),
            "slow_down" => AdjustTimerInterval(),
            "authorization_pending" => null,
            _ => CreateAuthResult(true, UserAuthMessage.Success, result)
        };

        if (_useAuthResult?.IsSuccess == false)
        {
            return false;
        }

        StopTimer();
        return true;
    }

    private static Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage> CreateAuthResult(
        bool isSuccess, UserAuthMessage message, OAuthData.ResponceData.UserAuthStateResponce result)
    {
        return new Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>(isSuccess, message, result);
    }

    private static Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage>? AdjustTimerInterval()
    {
        if (_timer != null) _timer.Interval += 1000;
        return null;
    }

    private static void StopTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        IsUserAuthed.Set();
    }
}