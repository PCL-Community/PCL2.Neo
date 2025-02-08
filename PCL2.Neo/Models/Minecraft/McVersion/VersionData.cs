using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

namespace PCL2.Neo.Models.Minecraft.McVersion
{
    namespace VersionManifest
    {
        public record Latest
        {
            public string Release { get; init; }
            public string Snapshot { get; init; }
        }

        public record VersionInfo
        {
            public string Id { get; init; }
            public string Time { get; init; }
            public string Type { get; init; }
            public string Url { get; init; }
            public string ReleaseTime { get; init; }
        }

        public record VersionManifestData
        {
            public Latest Latest { get; init; }
            public List<VersionInfo> Versions { get; init; }
        }
    }

    namespace VersionData
    {
        public record RulesFeaturesElements
        {
            public string Name { get; init; }
            public bool Value { get; init; }
        }

        public record GameRulesElements
        {
            public string Action { get; init; }
            public RulesFeaturesElements RulesFeature { get; init; }
        }

        public record GameRulesList
        {
            public List<GameRulesElements> Rules { get; init; }
            public List<string> Value { get; init; }
        }

        public record Game
        {
            public List<GameRulesList> GameRules { get; init; }
            public List<string> Value { get; init; }
        }

        public record OsElements
        {
            public string Name { get; init; }
            public string Value { get; init; }
        }

        public record JvmRulesElements
        {
            public string? FirstAction { get; set; }
            public string Action { get; set; }
            public List<OsElements> Os { get; set; }
        }

        public record JvmRulesList
        {
            public List<JvmRulesElements> Rules { get; init; }
            public List<string> Value { get; init; }
        }

        public record Jvm
        {
            public List<JvmRulesList> Rules { get; init; }
            public List<string> Value { get; init; }
        }

        public record Arguments
        {
            public Game Game { get; init; }
            public Jvm Jvm { get; init; }
        }

        public record AssetIndex
        {
            public string Id { get; init; }
            public string Sha1 { get; init; }
            public int Size { get; init; }
            public int TotalSize { get; init; }
            public string Url { get; init; }
        }

        public record DownloadsEneemltValue
        {
            public string Sha1 { get; init; }
            public int Size { get; init; }
            public string Url { get; init; }
        }

        public record Downloads
        {
            public DownloadsEneemltValue Client { get; init; }

            [JsonPropertyName("client_mappings")]
            public DownloadsEneemltValue ClientMappings { get; init; }

            public DownloadsEneemltValue Server { get; init; }

            [JsonPropertyName("server_mappings")]
            public DownloadsEneemltValue ServerMappings { get; init; }
        }

        public record JavaVersion
        {
            public string Component { get; init; }
            public int MajorVersion { get; init; }
        }

        public record Artifact
        {
            [JsonPropertyName("path")]
            public string Path { get; init; }

            [JsonPropertyName("sha1")]
            public string Sha1 { get; init; }

            [JsonPropertyName("size")]
            public int Size { get; init; }

            [JsonPropertyName("url")]
            public string Url { get; init; }
        }

        public record LibrariesDownloads
        {
            public Artifact? Artifact { get; set; }
            public List<(string, Artifact)>? Classifiers { get; set; }
            public string Name { get; set; }
            public List<(string, string)>? Natives { get; set; }
            public JvmRulesElements? Rules { get; set; }
            public List<string> Exclude { get; set; }
        }

        public record LibrariesNameUrl
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("checksums")]
            public List<string> CheckSums { get; set; }

            [JsonPropertyName("serverreq")]
            public bool? ServerReq { get; set; }

            [JsonPropertyName("clientreq")]
            public bool? ClientReq { get; set; }
        }

        public record Libraries
        {
            public List<LibrariesDownloads> Downloads { get; set; }
            public List<LibrariesNameUrl> NameUrls { get; set; }
        }

        public record File
        {
            public string Id { get; init; }
            public string Sha1 { get; init; }
            public int Size { get; init; }
            public string Url { get; init; }
        }

        public record Client
        {
            public string Argument { get; init; }
            public File File { get; init; }
            public string Type { get; init; }
        }

        public record Logging
        {
            public Client Client { get; init; }
        }

        public record Temp
        {
            [JsonPropertyName("assetIndex")]
            public AssetIndex AssetIndex { get; init; }

            [JsonPropertyName("assets")]
            public string Assets { get; init; }

            [JsonPropertyName("complianceLevel")]
            public byte ComplianceLevel { get; init; }

            [JsonPropertyName("downloads")]
            public Downloads Downloads { get; init; }

            [JsonPropertyName("id")]
            public string Id { get; init; }

            [JsonPropertyName("javaVersion")]
            public JavaVersion JavaVersion { get; init; }

            [JsonPropertyName("logging")]
            public Logging Logging { get; init; }

            [JsonPropertyName("mainClass")]
            public string MainClass { get; init; }

            [JsonPropertyName("minimumLauncherVersion")]
            public byte MinimunLauncherVersion { get; init; }

            [JsonPropertyName("releaseTime")]
            public string ReleaseTime { get; init; }

            [JsonPropertyName("time")]
            public string Time { get; init; }

            [JsonPropertyName("type")]
            public string Type { get; init; }

            [JsonPropertyName("clientVersion")]
            public string? ClientVersion { get; init; }
        }

        public record VersionData
        {
            [JsonPropertyName("arguments")]
            public Arguments? Arguments { get; set; }

            [JsonPropertyName("assetIndex")]
            public AssetIndex AssetIndex { get; set; }

            [JsonPropertyName("assets")]
            public string Assets { get; set; }

            [JsonPropertyName("complianceLevel")]
            public byte ComplianceLevel { get; set; }

            [JsonPropertyName("downloads")]
            public Downloads Downloads { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("javaVersion")]
            public JavaVersion JavaVersion { get; set; }

            [JsonPropertyName("libraries")]
            public Libraries Libraries { get; set; }

            [JsonPropertyName("logging")]
            public Logging Logging { get; set; }

            [JsonPropertyName("mainClass")]
            public string MainClass { get; set; }

            [JsonPropertyName("minimumLauncherVersion")]
            public byte MinimunLauncherVersion { get; set; }

            [JsonPropertyName("releaseTime")]
            public string ReleaseTime { get; set; }

            [JsonPropertyName("time")]
            public string Time { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("clientVersion")]
            public string? ClientVersion { get; set; }
        }
    }
}
