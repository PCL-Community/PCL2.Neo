using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.Java;

/// <summary>
/// 每一个 Java 实体的信息类
/// </summary>
public class JavaEntity
{
    /// <summary>
    /// 该 Java 实体的父目录，在构造时传入
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// 描述具体的 Java 信息，内部信息
    /// </summary>
    private readonly Lazy<JavaInfo> _javaInfo;

    /// <summary>
    /// 具体的 Java 信息数据结构
    /// </summary>
    private class JavaInfo
    {
        public int Version { get; set; }
        public bool Is64Bit { get; set; }
        public Architecture Architecture { get; set; }
        public bool IsJre { get; set; }
        public bool IsFatFile { get; set; }
        public bool IsCompatible { get; set; }
        public bool UseTranslation { get; set; }
    }

    /// <summary>
    /// 单个Java 实体的构造函数
    /// </summary>
    /// <param name="directoryPath">Java 可执行文件的父目录</param>
    public JavaEntity(string directoryPath)
    {
        DirectoryPath = directoryPath;
        _javaInfo = new Lazy<JavaInfo>(JavaInfoInit);
    }

    // 向外暴露的信息
    public bool IsUserImport { get; set; }
    public int Version => _javaInfo.Value.Version;
    public bool Is64Bit => _javaInfo.Value.Is64Bit;
    public Architecture Architecture => _javaInfo.Value.Architecture;
    public bool IsFatFile => _javaInfo.Value.IsFatFile;
    public bool IsCompatible => _javaInfo.Value.IsCompatible;
    public bool UseTranslation => _javaInfo.Value.UseTranslation;

    public bool IsJre => _javaInfo.Value.IsJre;

    public string JavaExe => Path.Combine(DirectoryPath, "java");

    public string JavaWExe => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? Path.Combine(DirectoryPath, "javaw.exe")
        : JavaExe;


    private JavaInfo JavaInfoInit()
    {
        var runJavaOutput = RunJava();
        var info = new JavaInfo();

        // 设置版本（Version）
        var regexMatch = Regex.Match(runJavaOutput, """version\s+"([\d._]+)""");
        var match = Regex.Match(regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty, @"^(\d+)");
        info.Version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
        if (info.Version == 1)
        {
            // java version 8
            match = Regex.Match(regexMatch.Groups[1].Value, @"^1\.(\d+)\.");
            info.Version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        // 设置位数（Is64Bit）
        regexMatch = Regex.Match(runJavaOutput, @"\b(\d+)-Bit\b"); // get bit
        info.Is64Bit = (regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty) == "64";

        // 设置是否为JDK/JRE
        var javacPath = System.IO.Path.Combine(DirectoryPath,
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javac.exe" : "javac");
        info.IsJre = !File.Exists(javacPath);

        // 设置二进制格式（Architecture），针对 macOS 的转译问题
        info.Architecture = RuntimeInformation.OSArchitecture;
        info.IsFatFile = false;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            using var lipoProcess = new Process();
            lipoProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/lipo",
                Arguments = "-info " + JavaExe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            lipoProcess.Start();
            lipoProcess.WaitForExit();

            var output = lipoProcess.StandardOutput.ReadToEnd();
            if (output.Trim().StartsWith("Non-fat file")) // fat file 在执行时架构和系统一致(同上)，所以这里判断不是 fat file 的情况
            {
                info.Architecture = output.Split(':').Last().Trim().Contains("arm64")
                    ? Architecture.Arm64
                    : Architecture.X64;
            }
            else
            {
                info.IsFatFile = true;
            }
        }
        // TODO 判断其他系统的可执行文件架构

        info.IsCompatible = info.Architecture == RuntimeInformation.OSArchitecture;
        info.UseTranslation = false;

        // if (info.IsCompatible == false)
        // {
        //     // 判断转译
        //     if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
        //         RuntimeInformation.OSArchitecture == Architecture.Arm64)
        //     {
        //         info.IsCompatible = true;
        //     }
        //     // TODO 判断其他系统的转译
        //
        //
        //     if (info.IsCompatible!.Value) // 若判断后变为兼容，则视为启用转译
        //     {
        //         info.UseTranslation = true;
        //     }
        // }
        return info;
    }

    /// <summary>
    /// 运行 java -version 并获取输出
    /// </summary>
    /// <returns></returns>
    private string RunJava()
    {
        using var javaProcess = new Process();
        javaProcess.StartInfo = new ProcessStartInfo
        {
            FileName = JavaExe,
            Arguments = "-version",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true, // 这个Java的输出流是tmd stderr！！！
            RedirectStandardOutput = true
        };
        javaProcess.Start();
        javaProcess.WaitForExit();

        var output = javaProcess.StandardError.ReadToEnd();
        return output != string.Empty ? output : javaProcess.StandardOutput.ReadToEnd(); // 就是tmd stderr
    }
}