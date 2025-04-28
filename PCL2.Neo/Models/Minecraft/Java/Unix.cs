using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Java
{
    /// <summary>
    /// 处理Unix系统下的java搜索
    /// 需要重新梳理一下逻辑：
    /// 1. 得到所有可能的 Java 目录 2. 检查其中是否有 Java 可执行文件 3. 归类返回，返回的是应是目录而非文件位置
    /// </summary>
    internal static class Unix
    {
        public static async Task<IEnumerable<JavaRuntime>> SearchJavaAsync(OSPlatform platform)
        {
            var javaPaths = new HashSet<string>();
            javaPaths.UnionWith(GetOsKnowDirs(platform));
            if (CheckJavaHome() is { } javaHome) javaPaths.Add(javaHome);
            if (CheckWithWhichJava() is { } whichJava) javaPaths.Add(whichJava);
            if (platform == OSPlatform.OSX) javaPaths.UnionWith(GetJavaHomesFromLibexec());
            var validPaths = new HashSet<string>();
            var validEntities = new List<JavaRuntime?>();
            foreach (string path in javaPaths.Where(Directory.Exists))
            {
                var foundPaths = await SearchJavaExecutablesAsync(path);
                foreach (string foundPath in foundPaths)
                {
                    var directoryName = Path.GetDirectoryName(foundPath);
                    if (directoryName != null)
                    {
                        validPaths.Add(directoryName);
                    }
                }
            }

            foreach (string validPath in validPaths)
            {
                var newEntity = await JavaRuntime.CreateJavaEntityAsync(validPath);
                if (newEntity is { Compability: not JavaCompability.Error })
                {
                    validEntities.Add(newEntity);
                }
            }

            return validEntities;
        }

        private static List<string> GetOsKnowDirs(OSPlatform platform)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var knowDirs = new List<string>();
            knowDirs.AddRange([
                "/usr/lib/jvm",
                "/usr/java",
                "/opt/java",
                "/opt/jdk",
                "/opt/jre",
                "/usr/local/java",
                "/usr/local/jdk",
                "/usr/local/jre",
                "/usr/local/opt",
                Path.Combine(homeDir, ".sdkman/candidates/java"),
            ]);
            if (platform == OSPlatform.OSX)
                knowDirs.AddRange([
                    "/Library/Java/JavaVirtualMachines",
                    $"{homeDir}/Library/Java/JavaVirtualMachines",
                    "/System/Library/Frameworks/JavaVM.framework/Versions", // Older macOS Java installs
                    "/opt/homebrew/opt/java", // Homebrew on Apple Silicon
                ]);
            return knowDirs.ConvertAll(Path.GetFullPath);
        }

        private static string? CheckJavaHome()
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            return string.IsNullOrEmpty(javaHome) ? null : javaHome;
        }

        private static string? CheckWithWhichJava()
        {
            var whichJava = RunCommand("which", "java");
            if (!File.Exists(whichJava))
                return null;
            var resolvedPath = Path.GetFullPath(whichJava);
            return Path.GetDirectoryName(resolvedPath);
        }


        static HashSet<string> GetJavaHomesFromLibexec()
        {
            var result = new HashSet<string>();
            var output = RunCommand("/usr/libexec/java_home", "-V");

            var regex = new Regex(@"(?<path>/Library/Java/JavaVirtualMachines/.*?/Contents/Home)");
            foreach (Match match in regex.Matches(output))
            {
                var homePath = match.Groups["path"].Value;
                var javaBin = Path.Combine(homePath, "bin", "java");
                if (File.Exists(javaBin))
                    result.Add(Path.GetDirectoryName(javaBin)!);
            }

            return result;
        }

        static string RunCommand(string command, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                var output = process!.StandardOutput.ReadToEnd();
                output += process.StandardError.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static Task<IEnumerable<string>> SearchJavaExecutablesAsync(string basePath)
        {
            var javaExecutables = new List<string>();
            try
            {
                var options = new EnumerationOptions
                {
                    RecurseSubdirectories = true, MaxRecursionDepth = 7, IgnoreInaccessible = true
                };

                var files = Directory.EnumerateFiles(basePath, "java", options);
                foreach (var file in files)
                {
                    if (IsValidJavaExecutableAsync(file))
                    {
                        javaExecutables.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Logger handling exceptions
                Console.WriteLine(ex);
            }

            return Task.FromResult<IEnumerable<string>>(javaExecutables);
        }

        private static bool IsValidJavaExecutableAsync(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}