using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Models.Minecraft.Java
{
    /// <summary>
    /// 处理Unix系统下的java搜索
    /// 需要重新梳理一下逻辑：
    /// 1. 得到所有可能的 Java 目录 2. 检查其中是否有 Java 可执行文件 3. 归类返回，返回的是应是目录而非文件位置
    /// </summary>
    internal static class Unix
    {
        public static async Task<IEnumerable<JavaRuntime>> SearchJavaAsync(SystemUtils.RunningOs platform)
        {
            var validPaths = new HashSet<string>();

            var toSearchPaths = new HashSet<string>();
            toSearchPaths.UnionWith(GetOsDirsToSearch(platform));
            if (CheckJavaHome() is { } javaHome) toSearchPaths.Add(javaHome);
            if (CheckWithWhichJava() is { } whichJava) validPaths.Add(whichJava);
            // if (platform is SystemUtils.RunningOs.MacOs) toSearchPaths.UnionWith(GetJavaHomesFromLibexec());
            validPaths.UnionWith(GetKnownDirsWithoutSearch(platform));

            var searchTasks = new List<Task<IEnumerable<string>>>();
            foreach (string path in toSearchPaths.Where(Directory.Exists))
                searchTasks.Add(SearchJavaExecutablesAsync(path));

            var foundPaths = await Task.WhenAll(searchTasks);
            foreach (IEnumerable<string> foundPath in foundPaths)
            foreach (string path in foundPath)
            {
                var directory = Path.GetDirectoryName(path);
                if (directory != null)
                    validPaths.Add(directory);
            }

            return (await Task.WhenAll(validPaths.Select(validPath => JavaRuntime.CreateJavaEntityAsync(validPath))))
                .Where(r => r is { Compability: not JavaCompability.Error })!;
        }

        private static List<string> GetOsDirsToSearch(SystemUtils.RunningOs platform)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var knowDirs = new List<string>();
            knowDirs.Add(Path.Combine(homeDir, ".sdkman/candidates/java"));
            
            if (platform is SystemUtils.RunningOs.Linux)
            {
                knowDirs.Add("/usr/lib/jvm");
                knowDirs.Add("/usr/java");
                knowDirs.Add("/opt/java");
                knowDirs.Add("/opt/jdk");
                knowDirs.Add("/opt/jre");
                knowDirs.Add("/usr/local/java");
                knowDirs.Add("/usr/local/jdk");
                knowDirs.Add("/usr/local/jre");
                knowDirs.Add("/usr/local/opt");
            }
            if (platform is SystemUtils.RunningOs.MacOs)
            {
                knowDirs.Add("/System/Library/Frameworks/JavaVM.framework/Versions"); // Older macOS Java installs
            }
            return knowDirs.ConvertAll(Path.GetFullPath);
        }

        private static List<string> GetKnownDirsWithoutSearch(SystemUtils.RunningOs platform)
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var knowDirs = new List<string>();
            if (platform is SystemUtils.RunningOs.MacOs)
            {
                knowDirs.Add("/usr/bin");
                
                string[] javaVmDirs = new string[]
                {
                    "/Library/Java/JavaVirtualMachines",
                    Path.Combine(homeDir, "/Library/Java/JavaVirtualMachines"),
                    "/opt/homebrew/opt/java/libexec",
                };
                
                foreach (var javaVmDir in javaVmDirs)
                {
                    if (!Directory.Exists(javaVmDir))
                        continue;
                        
                    foreach (var subDir in Directory.GetDirectories(javaVmDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        var binPath = Path.Combine(subDir, "Contents", "Home", "bin");
                        if (Directory.Exists(binPath))
                            knowDirs.Add(binPath);
                    }
                }
            }

            return knowDirs.ConvertAll(Path.GetFullPath);
        }

        private static string? CheckJavaHome()
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            Console.WriteLine("JAVA_HOME:" + javaHome);
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
                // 在.NET Standard 2.0中，Directory.EnumerateFiles不支持EnumerationOptions
                // 使用SearchOption.AllDirectories替代
                var files = Directory.EnumerateFiles(basePath, "java", SearchOption.AllDirectories);
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