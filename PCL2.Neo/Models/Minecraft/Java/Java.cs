using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PCL2.Neo.Models.Minecraft.Java
{
    /// <summary>
    /// 测试
    /// </summary>
    public static class Java
    {
        public static async Task<IEnumerable<JavaEntity>> SearchJava()
        {
            //switch (Environment.OSVersion.Platform)
            //{
            //    case PlatformID.Win32NT:
            //        javaList.AddRange(await JavaSearcher.Windows.SearchJava());
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
            // detail: .net type :    | system | performance
            //         .net framewrok:   macos | Not Support
            //         .net Core <= 3.1  macos | Unix
            //         .net 5+           macos | macosx
            // this problenm is fixed by follow code

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return await Windows.SearchJavaAsync();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return await Unix.SearchJavaAsync();

            throw new PlatformNotSupportedException();
        }
    }
}