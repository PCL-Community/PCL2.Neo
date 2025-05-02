using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account.Yggdrasil
{
    public class YggdrasilAuth : IAccount
    {
        /// <inheritdoc />
        public void Login()
        {
        }

        /// <inheritdoc />
        public Task Refresh(string refreshToken)
        {
        }

        /// <inheritdoc />
        public void ClearCache()
        {
        }

        /// <inheritdoc />
        public string GetSkin(string uuid)
        {
            return null;
        }

        /// <inheritdoc />
        public AccountInfo PlayOffline()
        {
            return null;
        }
    }
}
