using System;
using System.Collections.Generic;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    public record OfflineAccount() : BaseAccount(UserTypeConstants.Offline)
    {
        // Offline accounts typically don't have tokens.
        // All necessary properties are inherited from BaseAccount.

        // Constructor ensuring UserType is set correctly
    }
}