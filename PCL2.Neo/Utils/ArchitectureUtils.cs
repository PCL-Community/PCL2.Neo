using System.IO;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Utils;

public static class ArchitectureUtils
{
    public enum Architecture
    {
        X86,
        X64,
        Arm64,
        FatFile,
        Unknown
    }

    public static Architecture GetExecutableArchitecture(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Executable file not found.", path);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ReadPeHeader(path);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ReadElfHeader(path);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ReadMachOHeader(path);
        }

        return Architecture.Unknown;
    }

    private static Architecture ReadPeHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // Check MZ header
        if (reader.ReadUInt16() != 0x5A4D) // "MZ"
            return Architecture.Unknown;

        fs.Seek(0x3C, SeekOrigin.Begin);
        uint peHeaderOffset = reader.ReadUInt32();

        fs.Seek(peHeaderOffset, SeekOrigin.Begin);
        if (reader.ReadUInt32() != 0x00004550) // "PE\0\0"
            return Architecture.Unknown;

        // Optional Header Magic
        fs.Seek(peHeaderOffset + 0x16, SeekOrigin.Begin);
        ushort machine = reader.ReadUInt16();

        return machine switch
        {
            0x014C => Architecture.X86,       // IMAGE_FILE_MACHINE_I386
            0x8664 => Architecture.X64,       // IMAGE_FILE_MACHINE_AMD64
            0xAA64 => Architecture.Arm64,     // IMAGE_FILE_MACHINE_ARM64
            _ => Architecture.Unknown
        };
    }

    private static Architecture ReadElfHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        byte[] elfMagic = reader.ReadBytes(4);
        if (elfMagic[0] != 0x7F || elfMagic[1] != 'E' || elfMagic[2] != 'L' || elfMagic[3] != 'F')
            return Architecture.Unknown;

        fs.Seek(4, SeekOrigin.Begin); // Skip EI_MAG
        byte eiClass = reader.ReadByte(); // 1=32-bit, 2=64-bit

        // Skip data encoding (ei_data), version (ei_version), etc.
        fs.Seek(0x12, SeekOrigin.Begin); // e_machine offset for ELF32/ELF64
        ushort machine = reader.ReadUInt16();

        return machine switch
        {
            0x0003 => Architecture.X86,         // EM_386
            0x003E => Architecture.X64,         // EM_X86_64
            0x00B7 => Architecture.Arm64,       // EM_AARCH64
            _ => Architecture.Unknown
        };
    }

    private static Architecture ReadMachOHeader(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        uint magic = reader.ReadUInt32();

        // 判断是否为 Fat File
        if (magic == 0xBEBAFECA) // FAT_MAGIC or FAT_MAGIC_64
        {
            return Architecture.FatFile;
        }

        bool is64Bit = false;
        switch (magic)
        {
            case 0xFEEDFACE: // MH_MAGIC
            case 0xCEFAEDFE: // MH_CIGAM (reverse)
                break;
            case 0xFEEDFACF: // MH_MAGIC_64
            case 0xCFFAEDFE: // MH_CIGAM_64 (reverse)
                is64Bit = true;
                break;
            default:
                return Architecture.Unknown;
        }

        fs.Seek(is64Bit ? 4 : 0, SeekOrigin.Begin); // Skip magic
        uint cputype = reader.ReadUInt32();

        return cputype switch
        {
            0x7 => Architecture.X86,          // CPU_TYPE_I386
            0x1000007 => Architecture.X64,    // CPU_TYPE_X86_64
            0x100000C => Architecture.Arm64,  // CPU_TYPE_ARM64
            _ => Architecture.Unknown
        };
    }
}
