using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.Java.WindowsJava;

public static class JavaUtil {
    private const ushort PE_SIGNATURE = 23117;
    private const ushort IMAGE_FILE_MACHINE_IA64 = 267;
    private const ushort IMAGE_FILE_MACHINE_AMD64 = 523;
    private const uint PE_OPTIONAL_HEADER_SIGNATURE = 17744;

    public static JavaWinEntry GetJavaInfo(string path) {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
            return null;
        }

        try {
            string versionInfo = string.Empty;
            bool is64 = false;

            if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
                using var program = new Process {
                    StartInfo = new ProcessStartInfo {
                        Arguments = "-version",
                        FileName = path,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };

                program.Start();

                using var reader = program.StandardError;
                string output = reader.ReadToEnd();
                string pattern = "(java|openjdk) version \"\\s*(?<version>\\S+)\\s*\"";
                versionInfo = new Regex(pattern).Match(output).Groups["version"].Value;
                is64 = output.Contains("64-Bit");
            } else {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
                versionInfo = fileVersionInfo.ProductVersion!;
                is64 = GetIs64BitWindows(path)!;
            }

            string[] versionParts = versionInfo.Split(".");
            return new JavaWinEntry {
                Is64Bit = is64,
                JavaPath = path,
                JavaVersion = versionInfo,
                JavaDirectoryPath = Directory.GetParent(path).FullName,
                JavaSlugVersion = (int.Parse(versionParts[0]) == 1) ? int.Parse(versionParts[1]) : int.Parse(versionParts[0]),
            };
        } catch (Exception ex) {
            Console.WriteLine($"Error getting Java info: {ex.Message}");
            return null;
        }
    }

    private static bool GetIs64BitWindows(string path) {
        ushort architecture = 0;

        try {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream);

            if (binaryReader.ReadUInt16() == PE_SIGNATURE) {
                fileStream.Seek(0x3A, SeekOrigin.Current);
                fileStream.Seek(binaryReader.ReadUInt32(), SeekOrigin.Begin);

                if (binaryReader.ReadUInt32() == PE_OPTIONAL_HEADER_SIGNATURE) {
                    fileStream.Seek(20, SeekOrigin.Current);
                    architecture = binaryReader.ReadUInt16();
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error checking if file is 64 bit: {ex.Message}");
        }

        return architecture is IMAGE_FILE_MACHINE_AMD64 or IMAGE_FILE_MACHINE_IA64;
    }
}