using PCL2.Neo.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Service.MicrosoftAuth;

public static class DeviceCodeMode
{
    public record DeviceCodeInfo(string DeviceCode, string UserCode, string VerificationUri, int Interval);

    public record DeviceCodeAccessToken(string AccessToken, string RefreshToken, DateTimeOffset ExpiresIn);

    public record McAccountInfo(
        List<AccountInfo.Skin> Skins,
        List<AccountInfo.Cape> Capes,
        string UserName,
        string Uuid);
}