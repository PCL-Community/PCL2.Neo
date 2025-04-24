using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Java
{
    /// <summary>
    /// 处理Unix系统下的java
    /// </summary>
    internal static class Unix
    {
#warning "该方法未经过测试，可能无法正常工作 Unix/SearchJavaAsync"
        public static async Task<IEnumerable<JavaEntity>> SearchJavaAsync() =>
            await Task.Run(() => FindJavaExecutablePath().Select(it => new JavaEntity(it)));

        private static IEnumerable<string> FindJavaExecutablePath() =>
            GetPotentialJavaDir()
                .Where(Directory.Exists)
                .SelectMany(SearchJavaExecutables)
                .Select(Path.GetDirectoryName)
                .Distinct()!;

        private static bool IsValidJavaExecutable(string filePath)
        {
            // TODO: check execute permission
            return File.Exists(filePath);
        }

        private static IEnumerable<string> SearchJavaExecutables(string basePath)
        {
            try
            {
                return Directory
                    .EnumerateFiles(basePath, "java",
                        new EnumerationOptions
                        {
                            RecurseSubdirectories = true, MaxRecursionDepth = 7, IgnoreInaccessible = true
                        })
                    .Where(IsValidJavaExecutable);
            }
            catch (Exception)
            {
                // TODO: Logger handling exceptions
            }

            return [];
        }

        private static IEnumerable<string> GetPotentialJavaDir()
        {
            var paths = new List<string>();
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // add path
            paths.AddRange(Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? []);

            // add system paths
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                paths.AddRange([
                    "/usr/lib/jvm",
                    "/usr/java",
                    "/opt/java",
                    "/opt/jdk",
                    "/opt/jre",
                    "/usr/local/java",
                    "/usr/local/jdk",
                    "/usr/local/jre"
                ]);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                paths.AddRange([
                    "/Library/Java/JavaVirtualMachines",
                    $"{homeDir}/Library/Java/JavaVirtualMachines",
                    "/System/Library/Frameworks/JavaVM.framework/Versions", // Older macOS Java installs
                    "/usr/local/opt", // Homebrew links (e.g., /usr/local/opt/openjdk)
                    "/opt/homebrew/opt", // Homebrew on Apple Silicon
                    "/opt/java", // Manual installs sometimes go here too
                    "/opt/jdk",
                    "/opt/jre"
                ]);
            }
            else
            {
                // platform not supported
                throw new PlatformNotSupportedException();
            }

            // add home dirs

            if (!string.IsNullOrEmpty(homeDir))
            {
                paths.AddRange([
                    Path.Combine(homeDir, ".jdks"), // Common for SDKMAN
                    Path.Combine(homeDir, "java"),
                    Path.Combine(homeDir, ".java"), // Less common, but possible
                    Path.Combine(homeDir, "sdks", "java"), // IntelliJ default download location?),
                ]);
            }

            // add java home path
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (string.IsNullOrEmpty(javaHome) || !Directory.Exists(javaHome))
            {
                return paths.Distinct();
            }

            paths.Add(javaHome);
            // add java home parent path
            var parent =
                Path.GetDirectoryName(javaHome.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar));
            if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent) && !paths.Contains(parent))
            {
                paths.Add(parent);
            }

            return paths.Distinct();
        }
    }
}