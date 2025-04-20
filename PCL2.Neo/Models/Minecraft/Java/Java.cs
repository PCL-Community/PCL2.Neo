using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Java
{
    /// <summary>
    /// 测试
    /// </summary>
    public class Java
    {
        public static async Task<IEnumerable<JavaEntity>> SearchJava()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await Windows.SearchJavaAsync(); // TODO: Read setting to get whether full search or not.
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await Unix.SearchJava();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await Unix.SearchJava();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}