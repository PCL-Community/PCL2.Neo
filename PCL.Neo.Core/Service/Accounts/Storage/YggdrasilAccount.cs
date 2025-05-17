using System;
using System.Collections.Generic;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    public record YggdrasilAccount() : BaseAccount(UserTypeConstants.Yggdrasil)
    {
        // Yggdrasil accounts typically have an access token.
        // ClientToken is also common for Yggdrasil, but not present in the original Account record.
        // We'll stick to McAccessToken for now.
        public required string McAccessToken { get; init; }
        public required string ClientToken { get; init; } // Optional: Add if needed for Yggdrasil

        // Constructor ensuring UserType is set correctly
    }
}