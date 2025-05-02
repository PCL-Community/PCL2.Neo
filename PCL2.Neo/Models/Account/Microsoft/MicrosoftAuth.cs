using PCL2.Neo.Models.Account.OAuthService;
using System;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Microsoft;

public partial class MicrosoftAuth : IAccount
{
    public enum LoginType : byte
    {
        DeviceCode,
        AuthCode
    }

    public AccountInfo? AccountInfo;

    public async Task Login(LoginType type)
    {
        AccountInfo = type switch
        {
            LoginType.DeviceCode => await DeviceCodeLogin(),
            LoginType.AuthCode => await AuthCodeLogin(),

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <inheritdoc />
    public void Login()
    {
        throw new NotImplementedException("This method is not implemented. Please use Login(LoginType) to instead.");
    }

    public async Task Refresh(string refreshToken)
    {
        var tokens = await OAuth.RefreshToken(refreshToken);
        var minecraftAccessToken = await OAuth.GetMinecraftToken(tokens.AccessToken);
        var playerUuid = await OAuth.GetPlayerUuid(minecraftAccessToken);

        AccountInfo = new AccountInfo
        {
            AccessToken = minecraftAccessToken,
            OAuthToken =
                new AccountInfo.OAuthTokenData(tokens.AccessToken, tokens.RefreshToken,
                    new DateTimeOffset(DateTime.Today, TimeSpan.FromSeconds(tokens.ExpiresIn))),
            Uuid = playerUuid.Uuid,
            UserName = playerUuid.Name,
            UserType = AccountInfo.UserTypeEnum.UserTypeMsa,
            UserProperties = string.Empty
        };
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        AccountInfo = null;
    }

    /// <inheritdoc />
    public string GetSkin(string uuid, string savePath)
    {
    }

    /// <inheritdoc />
    public AccountInfo PlayOffline()
    {
        return null;
    }
}