using PCL2.Neo.Models.Minecraft.Java;

namespace PCL2.Neo.Models.Tests
{
    [TestClass]
    public class MainTests
    {
        [TestMethod]
        public async Task JavaSearchTest()
        {
            foreach (var javaEntity in await Java.SearchJava()) {
                Console.WriteLine(javaEntity.Path);
            }
        }
    }
}