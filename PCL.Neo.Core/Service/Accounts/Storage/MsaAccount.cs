using System;
using System.Collections.Generic;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    public record MsaAccount() : BaseAccount(UserTypeConstants.Msa)
    {
        public required OAuthTokenData OAuthToken { get; init; }
        public required string McAccessToken { get; init; }

        // Constructor ensuring UserType is set correctly
    }
}