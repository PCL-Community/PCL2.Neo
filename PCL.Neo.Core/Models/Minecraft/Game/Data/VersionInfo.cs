namespace PCL.Neo.Core.Models.Minecraft.Game.Data
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

    public enum VersionCardType : byte
    {
        Auto = 0,
        Hide = 1,
        Moddable = 2,
        Normal = 3,
        Unusual = 4,
        FoolsDay = 5,
        Error = 6,
    }

    public record GameVersionNum(byte Sub, byte? Fix = null) : IComparable<GameVersionNum>
    {
        private readonly (byte Major, byte Sub, int Fix) _version = (1, Sub, Fix ?? 0);

        public byte Major => _version.Major;
        public byte Sub => _version.Sub;
        public byte? Fix => _version.Fix > 0 ? (byte)_version.Fix : null;

        public int CompareTo(GameVersionNum? other)
        {
            return other == null ? 1 : (Major, Sub, Fix ?? 0).CompareTo((other.Major, other.Sub, other.Fix ?? 0));
        }

        public override string ToString()
        {
            return Fix.HasValue ? $"{Major}.{Sub}.{Fix}" : $"{Major}.{Sub}";
        }
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

    public enum McVersionState
    {
        Error,
        Vanilla,
        Snapshot,
        FoolsDay,
        OptiFine,
        Legacy,
        Forge,
        NeoForge,
        LiteLoader,
        Fabric,
    }
}
