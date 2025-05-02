using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account
{
    public interface IAccount
    {
        void Login();
        Task Refresh(string refreshToken);
        void ClearCache();
        string GetSkin(string uuid, string savePath);
        AccountInfo PlayOffline();
    }
}
