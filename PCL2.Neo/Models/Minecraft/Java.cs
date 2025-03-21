using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCL2.Neo.Models.Minecraft.JavaSearcher;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Models.Minecraft
{
    /// <summary>
    /// 测试
    /// </summary>
    public class Java
    {
        public static async Task<List<JavaEntity>> SearchJava()
        {
            var javaList = new List<JavaEntity>();

            //switch (Environment.OSVersion.Platform)
            //{
            //    case PlatformID.Win32NT:
            //        javaList.AddRange(await JavaSearcher.Windows.SearchJavaAsync());
            //        break;
            //    case PlatformID.Unix:
            //        javaList.AddRange(Unix.SerachJavaForLinuxAsync());
            //        break;
            //    case PlatformID.MacOSX:
            //        break;
            //    default:
            //        throw new PlatformNotSupportedException();
            //}

            // warning: Environment.OSVersion.Platform will have different performance in different .net planform
            // detail: .net framewrok:   macos | Not Support
            //         .net Core <= 3.1  macos | Unix
            //         .net 5+           macos | macosx
            // this problenm is fixed by follow code

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                javaList.AddRange(await JavaSearcher.Windows.SearchJavaAsync());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                javaList.AddRange(await Unix.SearchJavaAsync(PlatformID.Unix));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                javaList.AddRange(await Unix.SearchJavaAsync(PlatformID.MacOSX));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return javaList;
        }
    }
}
