using PCL2.Neo.Models.Minecraft.Java;

namespace PCL2.Neo.Models.Tests
{
    [TestClass]
    public class MainTests
    {
        [TestMethod]
        public async Task JavaSearchTest()
        {
            var result = Minecraft.Java.Java.SearchJava().Result;
            foreach (var item in result)
            {
                Console.WriteLine(item.Path);
            }
        }
    }
}