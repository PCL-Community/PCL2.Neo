using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account;

public interface IAutheticator
{
    // #TODO: remake this interface

    /// <summary>
    /// 更新OAuth的Token
    /// </summary>
    void RefreshOAuthToken();

    /// <summary>
    /// 更改MC的Token
    /// </summary>
    void RefreshMinecraftToken();

    /// <summary>
    /// 以离线模式游玩
    /// </summary>
    void PlayOffline();
}