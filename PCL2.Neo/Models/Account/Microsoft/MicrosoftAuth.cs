using PCL2.Neo.Models.Account.OAuthService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Microsoft;

public partial class MicrosoftAuth : IAccount
{
    public enum LoginType : byte
    {
        DeviceCode,
        AuthCode
    }

    // todo: refresh token
    // todo: storeage user profile
    public async Task<AccountInfo> Login(LoginType type) =>
        type switch
        {
            LoginType.DeviceCode => await DeviceCodeLogin(),
            LoginType.AuthCode => await AuthCodeLogin(),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    /// <inheritdoc />
    public AccountInfo Login()
    {
        throw new NotImplementedException("This method is not implemented. Please use Login(LoginType) to instead.");
    }

    public async Task<AccountInfo> Refresh(string refreshToken)
    {
        var tokens = await OAuth.RefreshToken(refreshToken);
        var minecraftAccessToken = await OAuth.GetMinecraftToken(tokens.AccessToken);
        var playerUuidAndName = await OAuth.GetPlayerUuidAndName(minecraftAccessToken);

        return new AccountInfo
        {
            AccessToken = minecraftAccessToken,
            RefreshToken = tokens.RefreshToken,
            Uuid = playerUuidAndName.Uuid,
            UserName = playerUuidAndName.Name,
            UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
            UserProperties = string.Empty
        };
    }

    /// <inheritdoc />
    public void ClearCache()
    {
    }

    /// <inheritdoc />
    public string GetSkins(string uuid)
    {
        return null;
    }

    /// <inheritdoc />
    public AccountInfo PlayOffline()
    {
        return null;
    }
}