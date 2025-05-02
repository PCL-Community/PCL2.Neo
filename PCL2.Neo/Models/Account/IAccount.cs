using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account
{
    public interface IAccount
    {
        AccountInfo Login();
        Task<AccountInfo> Refresh(string refreshToken);
        void ClearCache();
        string GetSkins(string uuid);
        AccountInfo PlayOffline();
    }
}
