using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft
{
    internal record JavaExist
    {
        public  bool IsExist { get; set; }

        public  string Path { get; set; }
    }
    public class JavaEntity(string path)
    {
        public string Path = path.EndsWith('\\') ? path : path + '\\';

        public bool IsUseable = true;

        private int? _version;

        public int Version
        {
            get
            {
                if (_version != null)
                {
                    return _version.Value;
                }

                // get java version code
                var javaVersionMatch = Regex.Match(Output, """version "([\d._]+)""");
                var match = Regex.Match(javaVersionMatch.Success ? javaVersionMatch.Groups[1].Value : string.Empty,
                    @"^(\d+)\.");
                _version = match.Success ? int.Parse(match.Groups[1].Value) : 0;

                if (_version == 1)
                {
                    // java version 8
                    match = Regex.Match(javaVersionMatch.Groups[1].Value, @"^1\.(\d+)\.");
                    _version = match.Success ? int.Parse(match.Groups[1].Value) : 0;

                    return _version.Value;
                }

                return _version.Value;
            }
        }
        public string JavaExe => Path + "java.exe";
        public string JavaWExe => Path + "javaw.exe";

        private string? _output;

        private string Output
        {
            get
            {
                if (_output != null)
                {
                    return _output;
                }

                _output = RunJava();
                return _output;
            }
        }

        public bool IsJre => !File.Exists(Path + "\\javac.exe");
        public bool IsUserImport { set; get; }

        private bool? _is64Bit;

        public bool Is64Bit
        {
            get
            {
                if (_is64Bit != null)
                {
                    return _is64Bit.Value;
                }

                var javaBitMatch = Regex.Match(Output, @"\b(\d+)-Bit\b"); // get bit
                _is64Bit = (javaBitMatch.Success ? javaBitMatch.Groups[1].Value : string.Empty) == "64";
                return _is64Bit.Value;
            }
        }

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

            var output = javaProcess.StandardError.ReadToEnd(); // check stderr have content
            return output != string.Empty ? output : javaProcess.StandardOutput.ReadToEnd(); // 就是tmd stderr
        }
    }
}
