using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.Java;
public enum JavaCompability
{
    Unknown,
    Yes,
    No,
    UnderTranslation,
}

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
    /// 描述具体的 Java 信息，内部信息，不应在外部取用
    /// </summary>
    private readonly Lazy<JavaInfo> _javaInfo;

    /// <summary>
    /// 具体的 Java 信息数据结构
    /// </summary>
    private class JavaInfo
    {
        public int Version { get; set; }
        public bool Is64Bit { get; set; }
        // public Architecture Architecture { get; set; }
        public bool IsJre { get; set; }
        public bool IsFatFile { get; set; }
        public JavaCompability Compability { get; set; }
        public required string JavaExe { get; set; }
        public required string JavaWExe { get; set; }
    }

    /// <summary>
    /// 单个Java 实体的构造函数
    /// </summary>
    /// <param name="directoryPath">Java 可执行文件的父目录</param>
    public JavaEntity(string directoryPath)
    {
        Debug.WriteLine($"创建 JavaEntity: {directoryPath}");
        DirectoryPath = directoryPath;
        _javaInfo = new Lazy<JavaInfo>(JavaInfoInit);
    }

    // 向外暴露的信息
    public bool IsUserImport { get; set; }
    public int Version => _javaInfo.Value.Version;
    public bool Is64Bit => _javaInfo.Value.Is64Bit;
    // public Architecture Architecture => _javaInfo.Value.Architecture;
    public bool IsFatFile => _javaInfo.Value.IsFatFile;
    public JavaCompability Compability => _javaInfo.Value.Compability;
    public bool IsJre => _javaInfo.Value.IsJre;
    public string JavaExe => Path.Combine(DirectoryPath, "java");   // [INFO] 这里必须直接指定，否则初始化会出错

    /// <summary>
    /// Windows 特有的 javaw.exe
    /// </summary>
    public string JavaWExe => _javaInfo.Value.JavaWExe;


    private JavaInfo JavaInfoInit()
    {
        Debug.WriteLine("JavaInfoInit...");
        var runJavaOutput = GetRunJavaOutput(JavaExe);
        var info = new JavaInfo
        {
            Version = MatchVersion(runJavaOutput), // 设置版本（Version）
            Is64Bit = MatchIs64Bit(runJavaOutput), // 设置位数（Is64Bit）
            IsJre = !File.Exists(Path.Combine(DirectoryPath,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javac.exe" : "javac")),
            // Architecture = RuntimeInformation.OSArchitecture,
            IsFatFile = false,
            Compability = JavaCompability.Unknown,
            JavaExe = JavaExe,
            JavaWExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(DirectoryPath, "javaw.exe")
                : JavaExe,
        };

        // 针对 Windows 设置兼容性，是 64 位则兼容
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            info.Compability = info.Is64Bit ? JavaCompability.Yes : JavaCompability.No;
        }

        // 针对 macOS 的转译问题额外设置兼容性
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
            var output = lipoProcess.StandardOutput.ReadToEnd().Trim().Split(":").Last();
            var sysArchitecture = RuntimeInformation.OSArchitecture;
            info.IsFatFile = !output.StartsWith("Non-fat file");
            switch (sysArchitecture)
            {
                case Architecture.X64:
                    info.Compability = output.Contains("x86_64") ? JavaCompability.Yes : JavaCompability.No;
                    break;
                case Architecture.Arm64:
                    if(output.Contains("arm64")) info.Compability = JavaCompability.Yes;
                    else if(output.Contains("x86_64")) info.Compability = JavaCompability.UnderTranslation;
                    break;
                default:
                    Debug.WriteLine("未知的 macOS 系统架构");  // 理论上程序不可能运行到这里
                    break;
            }
        }

        // 针对 Linux 设置兼容性
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            using var fileProcess = new Process();
            fileProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/file",
                Arguments = "-L " + JavaExe,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            fileProcess.Start();
            fileProcess.WaitForExit();
            var arch = fileProcess.StandardOutput.ReadToEnd().Trim().Replace(JavaExe, "").Split(",")[2];
            info.IsFatFile = false; // TODO 需要进一步判断
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64:
                    info.Compability = arch.Contains("x86-64") ? JavaCompability.Yes : JavaCompability.No;
                    break;
                case Architecture.Arm64:
                    if (arch.Contains("ARM aarch64"))
                        info.Compability = JavaCompability.Yes;
                    else if (arch.Contains("x86-64"))
                        info.Compability = JavaCompability.UnderTranslation; // QEMU
                    break;
                default:
                    Debug.WriteLine("未知的 Linux 系统架构");
                    break;
            }
        }

        // TODO)) 判断其他系统的可执行文件架构
        return info;
    }

    /// <summary>
    /// 运行 java -version 并获取输出
    /// </summary>
    /// <returns></returns>
    private static string GetRunJavaOutput(string javaExe)
    {
        using var javaProcess = new Process();
        javaProcess.StartInfo = new ProcessStartInfo
        {
            FileName = javaExe,
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

    private static int MatchVersion(string runJavaOutput)
    {
        var regexMatch = Regex.Match(runJavaOutput, """version\s+"([\d._]+)""");
        var match = Regex.Match(regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty, @"^(\d+)");
        int version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
        if (version == 1)
        {
            // java version 8
            match = Regex.Match(regexMatch.Groups[1].Value, @"^1\.(\d+)\.");
            version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        return version;
    }

    private static bool MatchIs64Bit(string runJavaOutput)
    {
        var regexMatch = Regex.Match(runJavaOutput, @"\b(\d+)-Bit\b"); // get bit
        return (regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty) == "64";
    }
}
