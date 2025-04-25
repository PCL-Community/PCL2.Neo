namespace PCL2.Neo.Models.Minecraft.Game.Data;

public record GameEntity
{
#nullable disable
    public GameVersion GameVersion { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string GamePath { get; set; }
    public Icons Icon { get; set; }
    public string JsonOrigContent { get; set; }
    public MetadataFile JsonContent { get; set; }
    public VersionType Type { get; set; }
    public ModLoader Loader { get; set; }
    public bool IsStared { get; set; } = false;
    /// <summary>
    /// Demonstrate is the version has been loader (runed).
    /// </summary>
    public bool IsLoadded { get; set; } = false;
#nullable enable
}