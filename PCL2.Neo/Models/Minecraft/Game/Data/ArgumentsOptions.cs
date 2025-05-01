using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Game.Data;

public class ArgumentsOptions
{
    /// <summary>
    /// The Custom Values for the arguments. Need init.
    /// </summary>
    public readonly Dictionary<string, string> ArgumentsCustomValue = new()
    {
        { "auth_player_name", string.Empty },
        { "version_name", string.Empty },
        { "game_directory", string.Empty },
        { "assets_root", string.Empty },
        { "assets_index_name", string.Empty },
        { "auth_uuid", string.Empty },
        { "auth_access_token", string.Empty },
        { "clientid", string.Empty }, // TODO: Set Client ID
        { "auth_xuid", string.Empty },
        { "user_properties", string.Empty },
        { "user_type", string.Empty },
        { "version_type", string.Empty },
        { "resolution_width", string.Empty },
        { "resolution_height", string.Empty },
        { "quickPlayPath", string.Empty },
        { "quickPlaySingleplayer", string.Empty },
        { "quickPlayMultiplayer", string.Empty },
        { "quickPlayRealms", string.Empty },
        { "natives_directory", string.Empty },
        { "launcher_name", "PCL2.Neo" },
        { "launcher_version", string.Empty },
        { "classpath", string.Empty }
    };

    public string UserPropertites
    {
        get => ArgumentsCustomValue["user_properties"];
        set => ArgumentsCustomValue["user_properties"] = value;
    }

    public string AuthPlayerName
    {
        get => ArgumentsCustomValue["auth_player_name"];
        set => ArgumentsCustomValue["auth_player_name"] = value;
    }

    public string VersionName
    {
        get => ArgumentsCustomValue["version_name"];
        set => ArgumentsCustomValue["version_name"] = value;
    }

    public string GameDirectory
    {
        get => ArgumentsCustomValue["game_directory"];
        set => ArgumentsCustomValue["game_directory"] = value;
    }

    public string AssetsRoot
    {
        get => ArgumentsCustomValue["assets_root"];
        set => ArgumentsCustomValue["assets_root"] = value;
    }

    public string AssetsIndexName
    {
        get => ArgumentsCustomValue["assets_index_name"];
        set => ArgumentsCustomValue["assets_index_name"] = value;
    }

    public string AuthUuid
    {
        get => ArgumentsCustomValue["auth_uuid"];
        set => ArgumentsCustomValue["auth_uuid"] = value;
    }

    public string AuthAccessToken
    {
        get => ArgumentsCustomValue["auth_access_token"];
        set => ArgumentsCustomValue["auth_access_token"] = value;
    }

    public string Clientid
    {
        get => ArgumentsCustomValue["clientid"];
        set => ArgumentsCustomValue["clientid"] = value;
    }

    public string AuthXuid
    {
        get => ArgumentsCustomValue["auth_xuid"];
        set => ArgumentsCustomValue["auth_xuid"] = value;
    }

    public string UserType
    {
        get => ArgumentsCustomValue["user_type"];
        set => ArgumentsCustomValue["user_type"] = value;
    }

    public string VersionType
    {
        get => ArgumentsCustomValue["version_type"];
        set => ArgumentsCustomValue["version_type"] = value;
    }

    public string ResolutionWidth
    {
        get => ArgumentsCustomValue["resolution_width"];
        set => ArgumentsCustomValue["resolution_width"] = value;
    }

    public string ResolutionHeight
    {
        get => ArgumentsCustomValue["resolution_height"];
        set => ArgumentsCustomValue["resolution_height"] = value;
    }

    public string QuickPlayPath
    {
        get => ArgumentsCustomValue["quickPlayPath"];
        set => ArgumentsCustomValue["quickPlayPath"] = value;
    }

    public string QuickPlaySingleplayer
    {
        get => ArgumentsCustomValue["quickPlaySingleplayer"];
        set => ArgumentsCustomValue["quickPlaySingleplayer"] = value;
    }

    public string QuickPlayMultiplayer
    {
        get => ArgumentsCustomValue["quickPlayMultiplayer"];
        set => ArgumentsCustomValue["quickPlayMultiplayer"] = value;
    }

    public string QuickPlayRealms
    {
        get => ArgumentsCustomValue["quickPlayRealms"];
        set => ArgumentsCustomValue["quickPlayRealms"] = value;
    }

    public string NativesDirectory
    {
        get => ArgumentsCustomValue["natives_directory"];
        set => ArgumentsCustomValue["natives_directory"] = value;
    }

    public string LauncherName
    {
        get => ArgumentsCustomValue["launcher_name"];
        set => ArgumentsCustomValue["launcher_name"] = value;
    }

    public string LauncherVersion
    {
        get => ArgumentsCustomValue["launcher_version"];
        set => ArgumentsCustomValue["launcher_version"] = value;
    }

    public string Classpath
    {
        get => ArgumentsCustomValue["classpath"];
        set => ArgumentsCustomValue["classpath"] = value;
    }
}