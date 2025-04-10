using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Models.Minecraft.Java
{
    /// <summary>
    /// 处理Unix系统下的java
    /// </summary>
    internal class Unix
    {
#warning "该方法未在 Linux 上测试，可能无法正常工作 Unix/SearchJava"
        public static IEnumerable<JavaEntity> SearchJava() =>
            FindJavaExecutablePath().Select(it => new JavaEntity(it));

        private static HashSet<string> FindJavaExecutablePath()
        {
            var foundJava = new HashSet<string>();

            GetPontentialJavaDir()
                .Where(Directory.Exists)
                .ToList().ForEach(it => SearchDirectoryForJava(it, foundJava));

            return foundJava;
        }

        private static bool IsValidJavaExecutable(string filePath)
        {
            if (Directory.Exists(filePath))
                return false;

            return !filePath.EndsWith(".jar") && !filePath.EndsWith(".zip") && !filePath.EndsWith(".so") &&
                   !filePath.EndsWith(".dylib");
        }

        private static void SearchDirectoryForJava(string basePath, HashSet<string> foundJava)
        {
            try
            {
                var binDirs = Directory.EnumerateDirectories(basePath, "bin", SearchOption.AllDirectories);
                binDirs
                    .SelectMany(binDir => Directory.EnumerateFiles(binDir, "java", SearchOption.TopDirectoryOnly))
                    .Where(IsValidJavaExecutable)
                    .ToList().ForEach(it => foundJava.Add(it));
            }
            catch (UnauthorizedAccessException) { }
        }

        private static IEnumerable<string> GetPontentialJavaDir()
        {
            var paths = new List<string>();
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

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