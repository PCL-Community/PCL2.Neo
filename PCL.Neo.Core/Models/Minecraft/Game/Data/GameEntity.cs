using System.Diagnostics;

namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record GameEntityInfo
{
#nullable disable
    /// <summary>
    /// The Game Version information.
    /// </summary>
    public GameVersionInfo GameVersion { get; set; }

    /// <summary>
    /// Game Name that is used to display in the UI.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Game Description that is used to display in the UI.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Game Folder Path.
    /// </summary>
    public string GamePath { get; set; }

    /// <summary>
    /// Game Root Path.
    /// </summary>
    public string RootPath { get; set; }

    /// <summary>
    /// Game Icon that is used to display in the UI.
    /// </summary>
    public Icons Icon { get; set; }

    /// <summary>
    /// The origin Game Json Content. Type is <see langword="string"/>.
    /// </summary>
    public string JsonOrigContent { get; set; }

    /// <summary>
    /// The Parsed Game Json Content. Type is <see cref="MetadataFile"/>.
    /// </summary>
    public MetadataFile JsonContent { get; set; }

    /// <summary>
    /// Demonstrate the Game Version Type.
    /// Content is <see cref="VersionType"/>.
    /// </summary>
    public VersionType Type { get; set; }

    /// <summary>
    /// If <see cref="Type"/> is <see cref="VersionType"/>.Modable, Loader will have value that is used to display in the UI.
    /// </summary>
    public ModLoader Loader { get; set; }

    /// <summary>
    /// Demonstrater is the game started by user. Used to display in the UI.
    /// </summary>
    public bool IsStared { get; set; } = false;
    /// <summary>
    /// Demonstrate is the version has been loader (runed).
    /// </summary>
    public bool IsLoadded { get; set; } = false;

    private bool? _isIndie { get; set; }

    public bool IsIndie
    {
        get
        {
            if (_isIndie != null)
            {
                return _isIndie.Value;
            }

            _isIndie = Path.Exists(Path.Combine(GamePath, "saves"))
                       && Path.Exists(Path.Combine(GamePath, "mods"));

            return _isIndie.Value;
        }
    }

    /// <summary>
    /// THe Game Jar File Path.
    /// </summary>
    public string JarPath { get; set; }
#nullable enable
}

public class GameEntity
{
    public required GameEntityInfo Entity { get; set; }
    public Process GameProcess { get; set; } = new();

    public GameEntity(GameEntityInfo entityInfo)
    {
        Entity = entityInfo;
        GameProcess.StartInfo = new ProcessStartInfo() { FileName = Entity.JarPath };
    }
}