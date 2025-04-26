using PCL2.Neo.Models.Minecraft.Java;

namespace PCL2.Neo.Tests.Models.Minecraft
{
    public class JavaTest
    {
        [Test]
        public async Task Test()
        {
            var javaEntities = await Java.SearchJava();
            foreach (JavaEntity javaEntity in javaEntities)
            {
                Console.WriteLine("--------------------");
                Console.WriteLine("路径: " + javaEntity.Path);
                Console.WriteLine("架构: " + javaEntity.Architecture);
                Console.WriteLine("启用转译: " + javaEntity.UseTranslation);
                Console.WriteLine("是否兼容: " + javaEntity.IsCompatible);
                Console.WriteLine("是否通用: " + javaEntity.IsFatFile);
            }
        }
    }
}