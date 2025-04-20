using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.Java
{
    public class JavaEntity(string path)
    {
        /// <summary>
        /// The java path
        /// </summary>
        public string Path = path;

        public bool IsUseable = true;

        private void JavaInfoInit()
        {
            // set version
            var regexMatch = Regex.Match(Output, """version "([\d._]+)""");
            var match = Regex.Match(regexMatch.Success ? regexMatch.Groups[1].Value : string.Empty,
                @"^(\d+)\.");
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

            // delete output
            _output = null;
        }

        private int? _version;

        /// <summary>
        /// Java version (e.g. 8, 17, 21, etc.)
        /// </summary>
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

        public string JavaExe => System.IO.Path.Combine(Path, "java.exe");
        public string JavaWExe => System.IO.Path.Combine(Path, "javaw.exe");

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

                var result = File.Exists(Path + "\\javac.exe");
                _isJre = result;
                return result;
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
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            javaProcess.Start();
            javaProcess.WaitForExit();

            var output = javaProcess.StandardError.ReadToEnd();
            return output != string.Empty ? output : javaProcess.StandardOutput.ReadToEnd();
        }
    }
}
