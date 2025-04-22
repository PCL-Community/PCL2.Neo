using System;
using System.IO;

namespace PCL2.Neo.Utils
{
    public class PeHeaderReader
    {
        public enum ImageFileMachine : ushort
        {
            UNKNOWN = 0x0,
            TARGET_HOST = 0x0001, // Useful for indicating the host machine type
            I386 = 0x014c, // Intel 386.
            R3000 = 0x0162, // MIPS little-endian, 0x160 big-endian
            R4000 = 0x0166, // MIPS little-endian
            R10000 = 0x0168, // MIPS little-endian
            WCEMIPSV2 = 0x0169, // MIPS little-endian WCE v2
            ALPHA = 0x0184, // Alpha_AXP
            SH3 = 0x01a2, // SH3 little-endian
            SH3DSP = 0x01a3,
            SH3E = 0x01a4, // SH3E little-endian
            SH4 = 0x01a6, // SH4 little-endian
            SH5 = 0x01a8, // SH5
            ARM = 0x01c0, // ARM Little-Endian
            THUMB = 0x01c2, // ARM Thumb/Thumb-2 Little-Endian
            ARMNT = 0x01c4, // ARM Thumb-2 Little-Endian
            AM33 = 0x01d3,
            POWERPC = 0x01F0, // IBM PowerPC Little-Endian
            POWERPCFP = 0x01f1,
            IA64 = 0x0200, // Intel 64
            MIPS16 = 0x0266, // MIPS
            ALPHA64 = 0x0284, // ALPHA64
            MIPSFPU = 0x0366, // MIPS
            MIPSFPU16 = 0x0466, // MIPS
            TRICORE = 0x0520, // Infineon
            CEF = 0x0CEF,
            EBC = 0x0EBC, // EFI Byte Code
            AMD64 = 0x8664, // AMD64 (x64)
            M32R = 0x9041, // M32R little-endian
            ARM64 = 0xAA64, // ARM64 Little-Endian
            CEE = 0xC0EE
        }

        private const ushort MzSignature = 0x5A4D;
        private const uint PeSignature = 0x00004550;
        private const int PeOffset = 0x3C;

        public static ushort GetMachine(string path)
        {
            //if (File.Exists(path))
            //{
            //    throw new FileNotFoundException("File not found." + path);
            //}
            // i dont konw that why dose there will throw exception even the file is exist...

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fileStream);

            if (reader.ReadUInt16() != MzSignature)
            {
                throw new BadImageFormatException("Invalid DOS header signature (MZ not found).");
            }

            fileStream.Seek(PeOffset, SeekOrigin.Begin);
            uint peOffset = reader.ReadUInt32();
            fileStream.Seek(peOffset, SeekOrigin.Begin);

            if (reader.ReadUInt32() != PeSignature)
            {
                throw new BadImageFormatException(@"Invalid PE header signature (PE\0\0 not found).");
            }

            ushort machine = reader.ReadUInt16();

            return machine;
        }

        public static ImageFileMachine GetMachineType(ushort machingId) => (ImageFileMachine)machingId;
    }
}
