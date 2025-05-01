using PCL2.Neo.Models.Minecraft.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static PCL2.Neo.Models.Minecraft.MetadataFile.Rule;

namespace PCL2.Neo.Models.Minecraft.Game;

public partial class Arguments
{
    public List<string> GameArguments { get; set; }

    private static readonly OsModel.ArchEnum CurrentArch = Const.Is64Os switch
    {
        true => OsModel.ArchEnum.X64,
        false => OsModel.ArchEnum.X86
    };

    private static readonly OsModel.NameEnum CurrentOs = Const.Os switch
    {
        Const.RunningOs.Windows => OsModel.NameEnum.Windows,
        Const.RunningOs.Linux => OsModel.NameEnum.Linux,
        Const.RunningOs.MacOs => OsModel.NameEnum.Osx,
        _ => OsModel.NameEnum.Unknown
    };

    private static readonly Dictionary<string, bool> GameRules = new()
    {
        { "is_demo_user", false },
        { "has_custom_resolution", true },
        { "has_quick_plays_support", false },
        { "is_quick_play_singleplayer", false },
        { "is_quick_play_multiplayer", false },
        { "is_quick_play_realms", false }
    };

    #region RuleMatch

    private static bool IsOsRuleAllow(MetadataFile.Rule rule)
    {
        if (rule.Os?.Name is null)
        {
            return true;
        }

        if (rule.Os?.Name == CurrentOs && rule.Action is ActionEnum.Allow or ActionEnum.Unknown)
        {
            return true;
        }

        if (rule.Os?.Name != CurrentOs && rule.Action is ActionEnum.Disallow or ActionEnum.Unknown)
        {
            return true;
        }

        return false;
    }

    private static bool IsArchRuleAllow(MetadataFile.Rule rule)
    {
        if (rule.Os?.Arch is null)
        {
            return true;
        }

        if (rule.Os?.Arch == CurrentArch && rule.Action is ActionEnum.Allow or ActionEnum.Disallow)
        {
            return true;
        }

        if (rule.Os?.Arch != CurrentArch && rule.Action is ActionEnum.Disallow or ActionEnum.Unknown)
        {
            return true;
        }

        return false;
    }

    private static bool IsGameFeatureAllow(MetadataFile.Rule rule)
    {
        if (rule.Features is null)
        {
            return true;
        }

        (string key, bool value) = rule.Features.FirstOrDefault();
        return GameRules[key] && rule.Action is ActionEnum.Allow or ActionEnum.Unknown;
    }

    private static bool GameArgumentsFilter(MetadataFile.Rule rule) =>
        IsGameFeatureAllow(rule) || IsOsRuleAllow(rule) || IsArchRuleAllow(rule);

    private static bool JvmArgumentsFilter(MetadataFile.Rule rule) =>
        IsOsRuleAllow(rule) || IsArchRuleAllow(rule);

    #endregion

    private static IEnumerable<string> ReplaceCustomValue(IEnumerable<string> arguments, ArgumentsOptions options)
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
#nullable disable
            if (options.ArgumentsCustomValue.TryGetValue(key, out string value))
            {
                CustomValueRegex().Replace(arg, value);
                result.Add(arg);
            }
            else
            {
                throw new ArgumentException($"Requied value ${key} not found.");
            }
#nullable enable
        }

        return result;
    }

    public Arguments(MetadataFile metadata, ArgumentsOptions options)
    {
        var arguments = metadata.Arguments;

        var gameArgu = arguments.Game
            .Where(it => it.Rules is null || GameArgumentsFilter(it.Rules.FirstOrDefault()!))
            .SelectMany(it => it.Value);

        var jvmArgu = arguments.Jvm
            .Where(it => it.Rules is null || JvmArgumentsFilter(it.Rules.FirstOrDefault()!))
            .SelectMany(it => it.Value);

        var gameResult = ReplaceCustomValue(gameArgu, options); // todo: add jvm argu and -D argu
        var jvmResult = ReplaceCustomValue(jvmArgu, options);

        GameArguments = gameResult.Concat(jvmResult).ToList();
    }

    /// <inheritdoc />
    public override string ToString() =>
        string.Join(" ", GameArguments);

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex CustomValueRegex();
}