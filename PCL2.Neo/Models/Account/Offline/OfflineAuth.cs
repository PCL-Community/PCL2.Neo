using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Offline;

public class OfflineAuth : IAuthenticator
{
    /// <inheritdoc />
    public void GetAccessToken()
    {
    }

    /// <inheritdoc />
    public void Refresh()
    {
    }

    /// <inheritdoc />
    public AccountInfo LogIn()
    {
        return null;
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