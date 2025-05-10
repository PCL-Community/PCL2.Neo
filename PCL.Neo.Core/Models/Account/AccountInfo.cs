namespace PCL.Neo.Core.Models.Account
{
    public record AccountInfo
    {
        public enum State
        {
            Active,
            Inactive
        }

        public required OAuthTokenData OAuthToken { get; init; }
        public required string McAccessToken { get; init; }
        public required string Uuid { get; set; }
        public required string UserName { get; init; }
        public required string UserType { get; init; }
        public string UserProperties { get; init; } = string.Empty;
        public required List<Skin> Skins { get; init; }
        public required List<Cape> Capes { get; init; }

        public record OAuthTokenData(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);

        public record Skin(string Id, Uri Url, string Variant, string TextureKey, State State);

        public record Cape(string Id, State State, Uri Url, string Alias);

        public static class UserTypeEnum
        {
            public const string Msa = "msa";
            public const string Mojang = "mojang";
            public const string Legacy = "legacy";
        }
    }
}