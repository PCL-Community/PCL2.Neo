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
        public static async Task<IEnumerable<JavaEntity>> SearchJava(bool fullSearch = false, int maxDeep = 7)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return
                    await Windows.SearchJavaAsync(fullSearch, maxDeep);
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