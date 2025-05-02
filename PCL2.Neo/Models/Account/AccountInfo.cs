using System;
using System.Collections.Generic;

namespace PCL2.Neo.Models.Account
{
    public record AccountInfo
    {
        public record OAuthTokenData(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

        public enum State
        {
            Active,
            Inactive
        }

        public record Skin(string Id, Uri Url, string Variant, string TextureKey, State State);

        public record Cape(string Id, State State, Uri Url, string Alias);

        public static class UserTypeEnum
        {
            public const string UserTypeMsa = "msa";
            public const string UserTypeMojang = "mojang";
            public const string UserTypeLegacy = "legacy";
        }

        public required OAuthTokenData OAuthToken { get; init; }
        public required string AccessToken { get; init; }
        public required string Uuid { get; set; }
        public required string UserName { get; init; }
        public required string UserType { get; init; }
        public required string UserProperties { get; init; }
        public required List<Skin> Skins { get; init; }
        public required List<Cape> Capes { get; init; }
    }
}
