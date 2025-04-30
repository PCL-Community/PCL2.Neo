namespace PCL2.Neo.Models.Minecraft.Java.WindowsJava;

public sealed record JavaWinEntry {
    public bool Is64Bit { get; init; }

    public string JavaPath { get; init; }

    public string JavaVersion { get; init; }

    public int JavaSlugVersion { get; init; }

    public string JavaDirectoryPath { get; init; }
}