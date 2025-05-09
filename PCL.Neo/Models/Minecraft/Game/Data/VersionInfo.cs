using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Models.Minecraft.Game.Data
{
    public enum Icons : byte
    {
        Auto = 0,
        Custom = 1,
        CubeStone = 2,
        CommandBlock = 3,
        GrassBlock = 4,
        EarthenPath = 5,
        Anvil = 6,
        RedStone = 7,
        RedStoneLightOff = 8,
        RedStoneLightOn = 9,
        Egg = 10,
        Fabric = 11,
        NeoForge = 12
    }

    public enum VersionType : byte
    {
        Auto = 0,
        Hide = 1,
        Modable = 2,
        Normal = 3,
        Unusual = 4,
        FoolDay = 5
    }

    public record GameVersion
    {
        public byte Major { get; set; } = 1;
        public byte Sub { get; set; }
        public byte Fix { get; set; }
    }

    public enum ModLoader : byte
    {
        None = 0,
        Forge = 1,
        Fabric = 2,
        NeoForge = 3,
        LiteLoader = 4,
        Rift = 5,
        Quilt = 6
    }
}
