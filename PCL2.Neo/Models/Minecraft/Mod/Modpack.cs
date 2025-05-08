using System;
using System.IO;
using System.IO.Compression;

namespace PCL2.Neo.Models.Minecraft.Mod;

public class ModPack
{
    public static void InstallPackModrinth(string mrpack, string directory)
    {
        if (!File.Exists(mrpack)) { throw new FileNotFoundException(); }
        // ZipFile.ExtractToDirectory(mrpack, directory);
        using (ZipArchive archive = ZipFile.OpenRead(mrpack))
        {
            var modrinthOptions = archive.GetEntry("modrinth.index.json");

            // foreach (ZipArchiveEntry entry in archive.Entries)
            // {
            //     if(entry.FullName == "modrinth.index.json")
            // }
        }
    }
}