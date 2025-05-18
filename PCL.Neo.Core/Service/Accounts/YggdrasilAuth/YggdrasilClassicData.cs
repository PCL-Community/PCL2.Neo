using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.YggdrasilAuth;

public static class YggdrasilClassicData
{
    public record ErrorInfo
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("cause")]
        public string? Cause { get; set; }
    }

    public record UserData
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
    }

    public record SelectedProfileData
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }

    public record PlayerProfile
    {
        public record PropertiesData
        {
            [JsonPropertyName("name")]
            public required string Name { get; set; }

            [JsonPropertyName("value")]
            public required string Value { get; set; }
        }

        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("properties")]
        public required List<PropertiesData> Properties { get; init; }
    }

    public record PlayerTexture
    {
        public class MetadataData
        {
            [JsonPropertyName("model")]
            public required string Model { get; set; }
        }

        public class SkinData
        {
            [JsonPropertyName("url")]
            public required string Url { get; set; }

            [JsonPropertyName("metadata")]
            public MetadataData? Metadata { get; set; }
        }

        public record CapeData
        {
            [JsonPropertyName("url")]
            public required string Url { get; set; }
        }

        public class TexturesData
        {
            [JsonPropertyName("SkinData")]
            public required SkinData Skin { get; set; }

            [JsonPropertyName("CAPE")]
            public CapeData? Cape { get; set; }
        }

        [JsonPropertyName("textures")]
        public required TexturesData Textures { get; set; }
    }

    public static class Request
    {
        public record Login
        {
            public record AgentData
            {
                [JsonPropertyName("name")]
                public required string Name { get; set; }

                [JsonPropertyName("version")]
                public required int Version { get; set; }
            }

            [JsonPropertyName("username")]
            public required string UserName { get; set; }

            [JsonPropertyName("password")]
            public required string Password { get; set; }

            [JsonPropertyName("requestUser")]
            public required bool Requestuser { get; set; } = false;

            [JsonPropertyName("agent")]
            public required AgentData Agent { get; set; }
        }

        public record Refresh
        {
            [JsonPropertyName("accessToken")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("clientToken")]
            public required string ClientToken { get; set; }

            [JsonPropertyName("requestUser")]
            public required bool RequestUser { get; set; } = false;

            [JsonPropertyName("selectedProfile")]
            public required SelectedProfileData SelectedProfile { get; set; }
        }

        public record Validata
        {
            [JsonPropertyName("accessToken")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("clientToken")]
            public required string ClientToken { get; set; }
        }

        public record Invalidata
        {
            [JsonPropertyName("accessToken")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("clientToken")]
            public required string ClientToken { get; set; }
        }

        public record Signout
        {
            [JsonPropertyName("username")]
            public required string UserName { get; set; }

            [JsonPropertyName("password")]
            public required string Password { get; set; }
        }
    }


    public static class Response
    {
        public record Login
        {
            public record AvailableProfileData
            {
                [JsonPropertyName("id")]
                public required string Id { get; set; }

                [JsonPropertyName("name")]
                public required string Name { get; set; }
            }

            [JsonPropertyName("accessToken")]
            public required string AccessToken { get; set; }

            [JsonPropertyName("clientToken")]
            public required string ClientToken { get; set; }

            [JsonPropertyName("availableProfiles")]
            public required List<AvailableProfileData> AvailableProfiles { get; set; }

            [JsonPropertyName("selectedProfile")]
            public SelectedProfileData? SelectedProfile { get; set; }

            [JsonPropertyName("user")]
            public UserData? User { get; set; }
        }

        public record Refresh
        {
            public required string  AccessToken { get; set; }
            public          string? ClientToken { get; set; }

            [JsonPropertyName("selectedProfile")]
            public SelectedProfileData? SelectedProfile { get; set; }

            [JsonPropertyName("user")]
            public UserData? User { get; set; }
        }

        public record MetaInfo
        {
            public record MetaData
            {
                public record LinksData
                {
                    [JsonPropertyName("announcement")]
                    public string? Announcement { get; set; }

                    [JsonPropertyName("homepage")]
                    public string? HomePage { get; set; }

                    [JsonPropertyName("register")]
                    public string? Register { get; set; }
                }

                [JsonPropertyName("serverName")]
                public string? ServerName { get; set; }

                [JsonPropertyName("implrmentationName")]
                public string? ImplementationName { get; set; }

                [JsonPropertyName("implementationVersion")]
                public string? ImplementationVersion { get; set; }

                [JsonPropertyName("links")]
                public LinksData? Links { get; set; }
            }

            [JsonPropertyName("meta")]
            public MetaData? Meta { get; set; }

            [JsonPropertyName("skinDomains")]
            public List<string>? SkinDomains { get; set; }
        };
    }
}