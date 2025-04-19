using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PCL2.Neo.Models.Minecraft.JavaSearcher;

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

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    javaList.AddRange(await PCL2.Neo.Models.Minecraft.JavaSearcher.Windows.SearchJava());
                    break;
                case PlatformID.Unix:
                    javaList.AddRange(Unix.SerachJava());
                    break;
                default:
                    throw new PlatformNotSupportedException();
            }

            return javaList;
        }
    }
}
