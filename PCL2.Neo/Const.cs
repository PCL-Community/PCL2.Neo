using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PCL2.Neo;

public static class Const
{
    /// <summary>
    /// 平台路径分隔符。
    /// </summary>
    public static readonly char Sep = System.IO.Path.DirectorySeparatorChar;

    /// <summary>
    /// 平台换行符。
    /// </summary>
    public static readonly string CrLf = Environment.NewLine;

    /// <summary>
    /// 程序的启动路径，以 <see cref="Sep"/> 结尾。
    /// </summary>
    public static readonly string Path = Environment.CurrentDirectory + Sep;

    /// <summary>
    /// 包含程序名的完整路径。
    /// </summary>
    public static readonly string PathWithName = Process.GetCurrentProcess().MainModule!.FileName;

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
}