using PCL2.Neo.Utils;
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
public static class DeviceCodeMode // todo: remake this device code auth
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
        Initialize(codePair.Interval, codePair.DeviceCode);

        IsUserAuthed.WaitOne();
        ValidateAuthResult();

        string accessToken = _useAuthResult!.Res.AccessToken;
        string refreshToken = _useAuthResult.Res.RefreshToken;
        var expires = _useAuthResult.Res.ExpiresIn;
        var minecraftAccessToken = await OAuth.GetMinecraftToken(accessToken);
        var playerInfo = await MinecraftInfo.GetPlayerUuid(minecraftAccessToken);

        return new AccountInfo
        {
            AccessToken = minecraftAccessToken,
            OAuthToken =
                new AccountInfo.OAuthTokenData(accessToken, refreshToken,
                    new DateTimeOffset(DateTime.Today, TimeSpan.FromSeconds(expires))),
            UserName = playerInfo.Name,
            UserProperties = string.Empty,
            UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
            Uuid = playerInfo.Uuid,
            Capes = MinecraftInfo.CollectCapes(playerInfo.Capes),
            Skins = MinecraftInfo.CollectSkins(playerInfo.Skins)
        };
    }

    private static FormUrlEncodedContent? _content;

    private static void Initialize(double interval, string deviceCode)
    {
        _timer = new Timer(interval) { AutoReset = true, Enabled = true };
        _timer.Elapsed += async (_, _) =>
        {
            /*
             * todo: Pre initialize request body
             * fix return value error
             */


            if (await PollingServer(_content!))
            {
                IsUserAuthed.Set();
            }
        };


        var data = OAuthData.FormUrlReqData.UserAuthStateData;
        data["device_code"] = deviceCode;

        _content = new FormUrlEncodedContent(data)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        _timer.Start();
    }

    private static void ValidateAuthResult()
    {
        ArgumentNullException.ThrowIfNull(_useAuthResult);

        if (_useAuthResult.IsSuccess == false)
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
        var content = new FormUrlEncodedContent(OAuthData.FormUrlReqData.DeviceCodeData)
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        return await Net.SendHttpRequestAsync<OAuthData.ResponceData.DeviceCodeResponce>(HttpMethod.Post,
            OAuthData.RequestUrls.DeviceCode, content, JsonSerializerOptions.Web);
    }

    private static async Task<bool> PollingServer(FormUrlEncodedContent content)
    {
        var result = await Net.SendHttpRequestAsync<OAuthData.ResponceData.UserAuthStateResponce>(HttpMethod.Post,
            OAuthData.RequestUrls.TokenUri, content, JsonSerializerOptions.Web);

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

        if (_useAuthResult == null)
        {
            return false;
        }

        StopTimer();
        return true;
    }

    private static Result<OAuthData.ResponceData.UserAuthStateResponce, UserAuthMessage> CreateAuthResult(
        bool isSuccess, UserAuthMessage message, OAuthData.ResponceData.UserAuthStateResponce result) =>
        new(isSuccess, message, result);

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