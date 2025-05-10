using System.Runtime.InteropServices;

namespace PCL.Neo.Core;

public static class Const
{
    /// <summary>
    /// 系统是否为64位。
    /// </summary>
    public static readonly bool Is64Os = Environment.Is64BitOperatingSystem;
    public enum RunningOs
    {
        Windows,
        Linux,
        MacOs,
        Unknown
    }

    public static readonly RunningOs Os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? RunningOs.Windows
        : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? RunningOs.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? RunningOs.MacOs
                : RunningOs.Unknown;

    public static readonly Architecture Architecture = RuntimeInformation.ProcessArchitecture;
}