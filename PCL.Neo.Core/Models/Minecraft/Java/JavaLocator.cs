using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PCL.Neo.Models.Minecraft.Java;

public static class JavaLocator
{
    public static string GetDefaultJavaPath()
    {
        var javaPath = FindJavaInstallations().FirstOrDefault();
        return javaPath ?? "java";
    }
    
    public static List<string> FindJavaInstallations()
    {
        var javaInstallations = new List<string>();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            javaInstallations.AddRange(FindWindowsJavaInstallations());
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            javaInstallations.AddRange(FindUnixJavaInstallations());
        }
        
        return javaInstallations;
    }
    
    private static IEnumerable<string> FindWindowsJavaInstallations()
    {
        var javaExecutables = new List<string>();
        
        // 检查程序文件目录
        var programFiles = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };
        
        foreach (var programFilesPath in programFiles)
        {
            if (string.IsNullOrEmpty(programFilesPath)) continue;
            
            // 检查Java目录
            var javaDir = Path.Combine(programFilesPath, "Java");
            if (Directory.Exists(javaDir))
            {
                foreach (var dir in Directory.GetDirectories(javaDir))
                {
                    var javaBinary = Path.Combine(dir, "bin", "javaw.exe");
                    if (File.Exists(javaBinary))
                    {
                        javaExecutables.Add(javaBinary);
                    }
                }
            }
            
            // 检查Oracle目录下的Java
            var oracleDir = Path.Combine(programFilesPath, "Oracle");
            if (Directory.Exists(oracleDir))
            {
                foreach (var dir in Directory.GetDirectories(oracleDir))
                {
                    if (dir.Contains("Java", StringComparison.OrdinalIgnoreCase))
                    {
                        var binDir = Path.Combine(dir, "bin");
                        if (Directory.Exists(binDir))
                        {
                            var javaBinary = Path.Combine(binDir, "javaw.exe");
                            if (File.Exists(javaBinary))
                            {
                                javaExecutables.Add(javaBinary);
                            }
                        }
                    }
                }
            }
        }
        
        // 检查注册表
        try
        {
            var javaRootKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment");
            if (javaRootKey != null)
            {
                var currentVersion = javaRootKey.GetValue("CurrentVersion")?.ToString();
                if (!string.IsNullOrEmpty(currentVersion))
                {
                    var javaRuntimeKey = javaRootKey.OpenSubKey(currentVersion);
                    if (javaRuntimeKey != null)
                    {
                        var javaHome = javaRuntimeKey.GetValue("JavaHome")?.ToString();
                        if (!string.IsNullOrEmpty(javaHome))
                        {
                            var javaBinary = Path.Combine(javaHome, "bin", "javaw.exe");
                            if (File.Exists(javaBinary))
                            {
                                javaExecutables.Add(javaBinary);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // 忽略注册表访问错误
        }
        
        // 去重并排序（优先选择最新版本）
        return javaExecutables.Distinct().OrderByDescending(path => path);
    }
    
    private static IEnumerable<string> FindUnixJavaInstallations()
    {
        var javaExecutables = new List<string>();
        
        // 检查常见的Java安装路径
        var commonPaths = new[]
        {
            "/usr/bin/java",
            "/usr/local/bin/java",
            "/opt/jdk/bin/java",
            "/opt/java/bin/java"
        };
        
        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
            {
                javaExecutables.Add(path);
            }
        }
        
        // 如果没有找到任何Java安装，则尝试使用"which java"命令
        if (!javaExecutables.Any())
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "which",
                        Arguments = "java",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(output) && File.Exists(output))
                {
                    javaExecutables.Add(output);
                }
            }
            catch
            {
                // 忽略命令执行错误
            }
        }
        
        return javaExecutables;
    }
} 