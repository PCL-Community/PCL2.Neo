using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account
{
    public interface IAuthenticator
    {
        AccountInfo LogIn();
        string GetIdentifier();
        void ClearCache();
        string GetCharchter();
        AccountInfo PlayOffline();
    }
}
