using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Offline;

public class OfflineAuth : IAccount
{
    /// <inheritdoc />
    public AccountInfo Login()
    {
        return null;
    }

    /// <inheritdoc />
    public void Refresh(string refreshToken)
    {
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