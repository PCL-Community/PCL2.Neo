using PCL2.Neo.Models.Minecraft.Java;
using PCL2.Neo.Utils;

namespace PCL2.Neo.Tests.Models.Minecraft
{
    public class JavaTest
    {
        [Test]
        public async Task Test()
        {
            JavaManager javaInstance = new();
            await javaInstance.JavaListInit();
            Console.WriteLine("当前有 " + javaInstance.JavaList.Count + " 个 Java");
            foreach (JavaRuntime? javaEntity in javaInstance.JavaList)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("路径: " + javaEntity.DirectoryPath);
                Console.WriteLine("是否兼容: " + javaEntity.Compability);
                Console.WriteLine("是否通用: " + javaEntity.IsFatFile);
            }
        }
    }
}