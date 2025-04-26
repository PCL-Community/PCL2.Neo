using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.Java
{
    public class JavaEntity(string path)
    {
        public readonly string Path = path;

        public bool IsUsable = true;

        private void JavaInfoInit()
        {
            // set version
            var regexMatch = Regex.Match(Output, """version\s+"([\d._]+)""");
            var match = Regex.Match(regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty,
                @"^(\d+)");
            _version = match.Success ? int.Parse(match.Groups[1].Value) : 0;

            if (_version == 1)
            {
                // java version 8
                match = Regex.Match(regexMatch.Groups[1].Value, @"^1\.(\d+)\.");
                _version = match.Success ? int.Parse(match.Groups[1].Value) : 0;
            }

            // set bit
            regexMatch = Regex.Match(Output, @"\b(\d+)-Bit\b"); // get bit
            _is64Bit = (regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty) == "64";

            _architecture = RuntimeInformation.OSArchitecture;

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
                if (output.Contains("Non-fat file")) // fat file 在执行时架构和系统一致(同上)，所以这里判断不是 fat file 的情况
                {
                    _architecture = output.Split(':').Last().Trim().Contains("arm64") ? Architecture.Arm64 : Architecture.X86;
                }
            }

            _isCompatible = _architecture == RuntimeInformation.OSArchitecture;
            _useTranslation = false;

            if (_isCompatible == false)
            {
                // 判断转译
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    _isCompatible = RuntimeInformation.OSArchitecture == Architecture.Arm64;
                }

                if (_isCompatible!.Value)
                {
                    _useTranslation = true;
                }
            }

            // delete output
            _output = null;
        }

        private bool? _useTranslation; // 是否启用转译，启用后会损失性能

        public bool UseTranslation
        {
            get
            {
                if (_useTranslation != null)
                {
                    return _useTranslation.Value;
                }

                JavaInfoInit();

                return _useTranslation!.Value;
            }
        }

        private bool? _isCompatible;

        public bool IsCompatible
        {
            get
            {
                if (_isCompatible != null)
                {
                    return _isCompatible.Value;
                }

                JavaInfoInit();

                return _isCompatible!.Value;
            }
        }

        private Architecture? _architecture;

        public Architecture Architecture
        {
            get
            {
                if (_architecture != null)
                {
                    return _architecture.Value;
                }

                JavaInfoInit();

                return _architecture!.Value;
            }
        }

        private int? _version;

        public int Version
        {
            get
            {
                if (_version != null)
                {
                    return _version.Value;
                }

                // java info init
                JavaInfoInit();

                return _version!.Value;
            }
        }

        public string JavaExe => System.IO.Path.Combine(Path, "java");

        public string? JavaWExe
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? System.IO.Path.Combine(Path, "javaw.exe") : null;

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

        private bool? _isJre;

        public bool IsJre
        {
            get
            {
                if (_isJre != null)
                {
                    return _isJre.Value;
                }

                var hasJavac = File.Exists(System.IO.Path.Combine(Path,
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javac.exe" : "javac"));
                _isJre = !hasJavac;
                return _isJre.Value;
            }
        }

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


                // java info init
                JavaInfoInit();

                return _is64Bit!.Value;
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

            var output = javaProcess.StandardError.ReadToEnd();
            return output != string.Empty ? output : javaProcess.StandardOutput.ReadToEnd(); // 就是tmd stderr
        }
    }
}
