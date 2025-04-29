using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#pragma warning disable CA1416 // Platform compatibility

namespace PCL2.Neo.Models.Minecraft.Java
{
    public static class Windows
    {
        private const int MaxDeep = 7;

        private static readonly IEnumerable<string> TargetSearchFolders =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime")
        ];

        public static async Task<IEnumerable<JavaRuntime>> SearchJavaAsync(bool fullSearch = false, int maxDeep = 7)
        {
            var foundPaths = new List<string>();

            foundPaths.AddRange(SearchRegister()); // search registries
            foundPaths.AddRange(SearchEnvironment()); // search path

            if (fullSearch) foundPaths.AddRange(await SearchDrives(maxDeep)); // full search mode

            var result = (await Task.WhenAll(
                TargetSearchFolders
                    .Where(Path.Exists)
                    .Select(item => SearchFolderAsync(item, maxDeep: 6))
            )).SelectMany(it => it);

            foundPaths.AddRange(result);
            var validRuntimeTasks = foundPaths.Distinct().Select(validPath => JavaRuntime.CreateJavaEntityAsync(validPath));
            var validEntities = await Task.WhenAll(validRuntimeTasks);
            return validEntities.Where(runtime => runtime is {Compability: not JavaCompability.No})!;
        }

        private static IEnumerable<string> SearchRegister()
        {
            using var javaSoftKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft");
            if (javaSoftKey == null) return [];

            var javaList = new List<string>();
            foreach (var subKeyName in javaSoftKey.GetSubKeyNames()) // iterate through all subkeys
            {
                using var subKey = javaSoftKey.OpenSubKey(subKeyName, RegistryKeyPermissionCheck.ReadSubTree);

                // get java home path
                var javaHomePath = subKey?.GetValue("JavaHome")?.ToString();
                if (javaHomePath == null) continue; // ignore not exist

                // get java path
                var javaPath = Path.Combine(javaHomePath, "bin");
                if (File.Exists(Path.Combine(javaPath, "java.exe"))) // select have java
                    javaList.Add(javaPath);
            }

            return javaList;
        }

        private static IEnumerable<string> SearchEnvironment()
        {
            var javaList = new List<string>();

            // search java home
            var javaHomePath = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (javaHomePath != null && File.Exists(Path.Combine(javaHomePath, "java.exe"))) // select have java
            {
                javaList.Add(javaHomePath);
            }

            // path
            Environment.GetEnvironmentVariable("Path")
                !.Split(';')
                .Where(Path.Exists)
                .Where(it => File.Exists(Path.Combine(it, "java.exe")))
                .ToList().ForEach(it => javaList.Add(it));

            return javaList;
        }

        private static readonly string[] TargetSubFolderWords =
        [
            "java", "jdk", "env", "环境", "run", "软件", "jre", "mc", "dragon", "soft", "cache", "temp", "corretto",
            "roaming", "users", "craft", "program", "世界", "net", "游戏", "oracle", "game", "file", "data", "jvm", "服务",
            "server", "客户", "client", "整合", "应用", "运行", "前置", "mojang", "官启", "新建文件夹", "eclipse", "microsoft",
            "hotspot", "runtime", "x86", "x64", "forge", "原版", "optifine", "官方", "启动", "hmcl", "mod", "download",
            "launch", "程序", "path", "version", "baka", "pcl", "zulu", "local", "packages", "国服", "网易", "ext", "netease",
            "启动"
        ];

        private static IEnumerable<string> SearchFolders(string folderPath, int deep, int maxDeep = MaxDeep)
        {
            if (deep >= maxDeep) return [];

            var javaList = new List<string>();

            if (File.Exists(Path.Combine(folderPath, "java.exe"))) javaList.Add(folderPath);

            var targetFolders = Directory.GetDirectories(folderPath)
                .Where(f => TargetSubFolderWords.Any(w => f.Contains(w.ToLower())));
            try
            {
                foreach (var folder in targetFolders)
                {
                    javaList.AddRange(SearchFolders(folder, deep + 1));
                }
            }
            catch (UnauthorizedAccessException) { }

            return javaList;
        }

        private static Task<IEnumerable<string>> SearchFolderAsync(
            string folderPath, int deep = 0, int maxDeep = MaxDeep)
            => Task.Run((() => SearchFolders(folderPath, deep, maxDeep)));

        private static Task<IEnumerable<string>> SearchDrives(int maxDeep)
        {
            var readyDrive = DriveInfo.GetDrives()
                .Where(d => d is { IsReady: true, DriveType: DriveType.Fixed })
                .Where(d => d.Name != @"C:\");

            var rootFolders = readyDrive
                .Select(d => d.RootDirectory)
                .Where(folder => !folder.Attributes.HasFlag(FileAttributes.ReparsePoint));

            var result = rootFolders
                .Select(async it => await SearchFolderAsync(it.FullName, maxDeep: maxDeep))
                .SelectMany(it => it.Result);

            return Task.FromResult(result);
        }
    }
}