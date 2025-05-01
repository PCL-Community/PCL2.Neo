using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Microsoft;

public partial class MicrosoftAuth : IAuthenticator
{
    public enum LoginType : byte
    {
        DeviceCode,
        AuthCode
    }

    // todo: refresh token
    // todo: storeage user profile
    public AccountInfo Login(LoginType type) =>
        type switch
        {
            LoginType.DeviceCode => DeviceCodeLogin(),
            LoginType.AuthCode => AuthCodeLogin(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    /// <inheritdoc />
    public AccountInfo Login()
    {
        throw new NotImplementedException("This method is not implemented. Please use Login(LoginType) to instead.");
    }

    /// <inheritdoc />
    public string GetIdentifier()
    {
        return null;
    }

    /// <inheritdoc />
    public void ClearCache()
    {
    }

    /// <inheritdoc />
    public string GetCharchter()
    {
        return null;
    }

    /// <inheritdoc />
    public AccountInfo PlayOffline()
    {
        return null;
    }
}