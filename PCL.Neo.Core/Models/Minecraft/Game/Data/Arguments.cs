using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data
{
    public partial class Arguments
    {
        [JsonPropertyName("game")]
        public List<object>? Game { get; set; }

        [JsonPropertyName("jvm")]
        public List<object>? Jvm { get; set; }

        public List<string> GameArguments { get; init; } = new();

        public enum OsArchType
        {
            X86,
            X64
        }

        public enum OsNameType
        {
            Windows,
            Linux,
            Osx,
            Unknown
        }

        private static readonly OsArchType CurrentArch = Environment.Is64BitOperatingSystem 
            ? OsArchType.X64 
            : OsArchType.X86;

        private static readonly OsNameType CurrentOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? OsNameType.Windows 
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) 
                ? OsNameType.Linux 
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) 
                    ? OsNameType.Osx 
                    : OsNameType.Unknown;

        private static readonly Dictionary<string, bool> GameRules = new()
        {
            { "is_demo_user", false },
            { "has_custom_resolution", true },
            { "has_quick_plays_support", false },
            { "is_quick_play_singleplayer", false },
            { "is_quick_play_multiplayer", false },
            { "is_quick_play_realms", false }
        };

        /// <summary>
        /// The Custom Values for the arguments. Need init.
        /// </summary>
        public static Dictionary<string, string> ArgumentsCustomValue = new()
        {
            { "auth_player_name", string.Empty },
            { "version_name", string.Empty },
            { "game_directory", string.Empty },
            { "assets_root", string.Empty },
            { "assets_index_name", string.Empty },
            { "auth_uuid", string.Empty },
            { "auth_access_token", string.Empty },
            { "clientid", string.Empty },
            { "auth_xuid", string.Empty },
            { "user_type", string.Empty },
            { "version_type", string.Empty },
            { "resolution_width", string.Empty },
            { "resolution_height", string.Empty },
            { "quickPlayPath", string.Empty },
            { "quickPlaySingleplayer", string.Empty },
            { "quickPlayMultiplayer", string.Empty },
            { "quickPlayRealms", string.Empty },
            { "natives_directory", string.Empty },
            { "launcher_name", string.Empty },
            { "launcher_version", string.Empty },
            { "classpath", string.Empty }
        };

        private static bool IsOsRuleAllow(Rule rule)
        {
            if (rule.Os?.Name is null)
            {
                return true;
            }

            bool isCurrentOs = rule.Os.Name == CurrentOs.ToString().ToLower();
            return (isCurrentOs && rule.Action == "allow") || (!isCurrentOs && rule.Action == "disallow");
        }

        private static bool IsArchRuleAllow(Rule rule)
        {
            if (rule.Os?.Arch is null)
            {
                return true;
            }

            bool isCurrentArch = rule.Os.Arch == (CurrentArch == OsArchType.X64 ? "64" : "32");
            return (isCurrentArch && rule.Action == "allow") || (!isCurrentArch && rule.Action == "disallow");
        }

        private static bool IsGameFeatureAllow(Rule rule)
        {
            return true;
        }

        private static bool GameArgumentsFilter(Rule rule) =>
            IsGameFeatureAllow(rule) || IsOsRuleAllow(rule) || IsArchRuleAllow(rule);

        private static bool JvmArgumentsFilter(Rule rule) =>
            IsOsRuleAllow(rule) || IsArchRuleAllow(rule);

        private static IEnumerable<string> ReplaceCustomValue(IEnumerable<string> arguments)
        {
            List<string> result = [];
            foreach (var arg in arguments)
            {
                var match = CustomValueRegex().Match(arg);
                if (!match.Success)
                {
                    result.Add(arg);
                    continue;
                }

                var key = match.Groups[1].Value;
                if (ArgumentsCustomValue.TryGetValue(key, out string? value))
                {
                    CustomValueRegex().Replace(arg, value);
                    result.Add(arg);
                }
                else
                {
                    throw new ArgumentException($"Requied value ${key} not found.");
                }
            }

            return result;
        }

        public Arguments()
        {
            Game = new List<object>();
            Jvm = new List<object>();
            GameArguments = new List<string>();
        }

        /*
        public Arguments(MetadataFile metadata)
        {
            Game = new List<object>();
            Jvm = new List<object>();

            var arguments = metadata.Arguments;

            var gameArgu = arguments.Game
                .Where(it => it.Rules is null || GameArgumentsFilter(it.Rules.FirstOrDefault()!))
                .SelectMany(it => it.Value);

            var jvmArgu = arguments.Jvm
                .Where(it => it.Rules is null || JvmArgumentsFilter(it.Rules.FirstOrDefault()!))
                .SelectMany(it => it.Value);

            var gameResult = ReplaceCustomValue(gameArgu);
            var jvmResult = ReplaceCustomValue(jvmArgu);

            GameArguments = gameResult.Concat(jvmResult).ToList();
        }
        */

        /// <inheritdoc />
        public override string ToString() =>
            string.Join(" ", GameArguments);

        [GeneratedRegex(@"\$\{([^}]+)\}")]
        private static partial Regex CustomValueRegex();
    }

    public class ArgRule
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "allow";

        [JsonPropertyName("features")]
        public Dictionary<string, bool>? Features { get; set; }

        [JsonPropertyName("os")]
        public OsRule? Os { get; set; }
    }
}
