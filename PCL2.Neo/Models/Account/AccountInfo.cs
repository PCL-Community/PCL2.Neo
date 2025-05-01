using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Account
{
    public record AccountInfo
    {
        public static class UserTypeEnum
        {
            public const string UserTypeMsa = "msa";
            public const string UserTypeMojang = "mojang";
            public const string UserTypeLegacy = "legacy";
        }

        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required string Uuid { get; set; }
        public required string UserName { get; init; }
        public required string UserType { get; init; }
        public required string UserProperties { get; init; }
    }
}
