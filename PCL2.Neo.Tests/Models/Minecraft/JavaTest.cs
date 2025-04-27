using PCL2.Neo.Models.Minecraft.Java;

namespace PCL2.Neo.Tests.Models.Minecraft
{
    public class JavaTest
    {
        [Test]
        public async Task Test()
        {
            Java javaInstance = await Java.CreateAsync();
            Console.WriteLine("搜索到 " + javaInstance.JavaList.Count + " 个 Java");
            foreach (JavaEntity javaEntity in javaInstance.JavaList)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("路径: " + javaEntity.DirectoryPath);
                Console.WriteLine("是否兼容: " + javaEntity.Compability);
                Console.WriteLine("是否通用: " + javaEntity.IsFatFile);
            }
        }
    }
}