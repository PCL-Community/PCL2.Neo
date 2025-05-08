using PCL2.Neo.Models.Minecraft.Java;
using PCL2.Neo.Utils;

namespace PCL2.Neo.Tests.Models.Minecraft
{
    public class JavaTest
    {
        [Test]
        public async Task Test()
        {
            Java javaInstance = await Java.CreateAsync();
            Console.WriteLine("当前有 " + javaInstance.JavaList.Count + " 个 Java");
            foreach (JavaEntity javaEntity in javaInstance.JavaList)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("路径: " + javaEntity.DirectoryPath);
                Console.WriteLine("架构: " + ArchitectureUtils.GetExecutableArchitecture(javaEntity.JavaExe));
            }
        }
    }
}