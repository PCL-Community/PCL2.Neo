using PCL2.Neo.Models.Account;
using PCL2.Neo.Models.Account.OAuthService;
using PCL2.Neo.Service.Exceptions;
using PCL2.Neo.Utils;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Service.MicrosoftAuth
{
    public class MicrosoftAuthService : IMicrosoftAuthService
    {
        /// <inheritdoc />
        public IObservable<DeviceFlowState> StartDeviceCodeFlow() =>
            Observable.Create<DeviceFlowState>(async (observer) =>
            {
                // get device code
                var deviceCodeResult = await RequestDeviceCodeAsync();
                if (deviceCodeResult.IsFailure)
                {
                    observer.OnError(deviceCodeResult.Error.Exception!);
                    return;
                }

                var deviceCodeInfo = deviceCodeResult.Value;

                // show for user and open browser
                OpenBrowserAsync(deviceCodeInfo.VerificationUri);
                observer.OnNext(new DeviceFlowAwaitUser(deviceCodeInfo.UserCode, deviceCodeInfo.VerificationUri));

                // polling server
                var tokenResult = await PollForTokenAsync(deviceCodeInfo.DeviceCode, deviceCodeInfo.Interval);
                observer.OnNext(new DeviceFlowPolling());

                if (tokenResult.IsFailure)
                {
                    observer.OnNext(tokenResult.Error);
                    return;
                }

                var tokenInfo = tokenResult.Value;

                // get user mc token
                var mcToken = await GetUserMinecraftAccessTokenAsync(tokenInfo.AccessToken);
                if (mcToken.IsFailure)
                {
                    observer.OnError(mcToken.Error);
                    return;
                }

                // get user account info
                var accountInfoResult = await GetUserAccountInfo(mcToken.Value);
                if (accountInfoResult.IsFailure)
                {
                    observer.OnError(accountInfoResult.Error!);
                    return;
                }

                var accountInfo = accountInfoResult.Value;

                var account = new AccountInfo
                {
                    McAccessToken = mcToken.Value,
                    UserName = accountInfo.UserName,
                    UserType = AccountInfo.UserTypeEnum.Msa,
                    OAuthToken =
                        new AccountInfo.OAuthTokenData(tokenInfo.AccessToken, tokenInfo.RefreshToken,
                            tokenInfo.ExpiresIn),
                    Uuid = accountInfo.Uuid,
                    Skins = accountInfo.Skins,
                    Capes = accountInfo.Capes
                };

                observer.OnNext(new DeviceFlowSucceeded(account));
                observer.OnCompleted();
            });

        private static void OpenBrowserAsync(string requiredUrl)
        {
            var processStartInfo = new ProcessStartInfo { FileName = requiredUrl, UseShellExecute = true };

            Process.Start(processStartInfo);
        }

        /// <inheritdoc />
        public async Task<Result<DeviceCodeMode.DeviceCodeInfo, HttpError>> RequestDeviceCodeAsync()
        {
            var content = new FormUrlEncodedContent(OAuthData.FormUrlReqData.DeviceCodeData)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
            };

            try
            {
                var temp = await Net.SendHttpRequestAsync<DeviceCodeMode.DeviceCodeInfo>(HttpMethod.Post,
                    OAuthData.RequestUrls.DeviceCode, content, JsonSerializerOptions.Web);
                var resutlt = new DeviceCodeMode.DeviceCodeInfo(temp.DeviceCode, temp.UserCode, temp.VerificationUri,
                    temp.Interval);
                return Result<DeviceCodeMode.DeviceCodeInfo, HttpError>.Ok(resutlt);
            }
            catch (HttpRequestException e)
            {
                return Result<DeviceCodeMode.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                    "Network error while requesting device code.", Exception: e));
            }
            catch (JsonException e)
            {
                return Result<DeviceCodeMode.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                    "Failed to parse device code response.", Exception: e));
            }
            catch (Exception e)
            {
                return Result<DeviceCodeMode.DeviceCodeInfo, HttpError>.Fail(new HttpError(null,
                    "An unexpected error occurred.", Exception: e));
            }
        }

        /// <inheritdoc />
        public async Task<Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>> PollForTokenAsync(
            string deviceCode, int interval)
        {
            var tempInterval = interval;
            var content = OAuthData.FormUrlReqData.UserAuthStateData;
            content["device_code"] = deviceCode;
            var msg = new FormUrlEncodedContent(content)
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
            };

            while (true)
            {
                try
                {
                    await Task.Delay(tempInterval);

                    var tempResult =
                        await Net.SendHttpRequestAsync<OAuthData.ResponseData.UserAuthStateResponse>(HttpMethod.Post,
                            OAuthData.RequestUrls.TokenUri, msg, JsonSerializerOptions.Web);

                    // handle response
                    if (!string.IsNullOrEmpty(tempResult.Error))
                    {
                        switch (tempResult.Error)
                        {
                            case "authorization_declined":
                                return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                    new DeviceFlowError(new DeviceFlowDeclined(), null));
                            case "expired_token":
                                return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                    new DeviceFlowError(new DeviceFlowExpired(), null));
                            case "bad_verification_code":
                                return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                    new DeviceFlowError(new DeviceFlowDeclined(), null));
                            case "slow_down":
                                tempInterval = Math.Min(tempInterval * 2, 900); // Adjust polling interval
                                continue;
                            case "authorization_pending":
                                continue; // Keep polling
                            default:
                                return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                                    new DeviceFlowError(new DeviceFlowUnkonw(), null));
                        }
                    }

                    // create result
                    var result = new DeviceCodeMode.DeviceCodeAccessToken(tempResult.AccessToken,
                        tempResult.RefreshToken,
                        DateTimeOffset.UtcNow.AddSeconds(tempResult.ExpiresIn));

                    return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Ok(result);
                }
                catch (HttpRequestException e)
                {
                    return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                        new DeviceFlowError(new DeviceFlowInternetError(), e));
                }
                catch (JsonException e)
                {
                    return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                        new DeviceFlowError(new DeviceFlowJsonError(), e));
                }
                catch (Exception e)
                {
                    return Result<DeviceCodeMode.DeviceCodeAccessToken, DeviceFlowError>.Fail(
                        new DeviceFlowError(new DeviceFlowUnkonw(), e));
                }
            }
        }

        /// <inheritdoc />
        public async Task<Result<string, MinecraftInfo.NotHaveGameException>> GetUserMinecraftAccessTokenAsync(
            string accessToken)
        {
            try
            {
                var minecraftToken = await OAuth.GetMinecraftToken(accessToken);
                return Result<string, MinecraftInfo.NotHaveGameException>.Ok(minecraftToken);
            }
            catch (MinecraftInfo.NotHaveGameException e)
            {
                return Result<string, MinecraftInfo.NotHaveGameException>.Fail(
                    new MinecraftInfo.NotHaveGameException(e.Message));
            }
        }

        /// <inheritdoc />
        public async Task<Result<DeviceCodeMode.McAccountInfo, Exception>> GetUserAccountInfo(string accessToken)
        {
            try
            {
                var playerInfo = await MinecraftInfo.GetPlayerUuid(accessToken);
                var capes = MinecraftInfo.CollectCapes(playerInfo.Capes);
                var skins = MinecraftInfo.CollectSkins(playerInfo.Skins);
                var uuid = playerInfo.Uuid;

                return Result<DeviceCodeMode.McAccountInfo, Exception>.Ok(
                    new DeviceCodeMode.McAccountInfo(skins, capes, playerInfo.Name, uuid));
            }
            catch (Exception e)
            {
                return Result<DeviceCodeMode.McAccountInfo, Exception>.Fail(e);
            }
        }

        /// <inheritdoc />
        public async Task<Result<AccountInfo.OAuthTokenData, Exception>> RefreshTokenAsync(string refreshToken)
        {
            var newToken = await OAuth.RefreshToken(refreshToken);
            try
            {
                var newTokenData = new AccountInfo.OAuthTokenData(newToken.AccessToken, newToken.RefreshToken,
                    new DateTimeOffset(DateTime.Today, TimeSpan.FromSeconds(newToken.ExpiresIn)));

                return Result<AccountInfo.OAuthTokenData, Exception>.Ok(newTokenData);
            }
            catch (Exception e)
            {
                return Result<AccountInfo.OAuthTokenData, Exception>.Fail(e);
            }
        }
    }
}