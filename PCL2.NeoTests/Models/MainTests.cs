namespace PCL2.Neo.Models.Tests
{
    [TestClass]
    public class MainTests
    {
        [TestMethod]
        public void JavaSearchTest()
        {
            var result = Minecraft.Java.Java.SearchJava().Result;
            foreach (var item in result)
            {
                Console.WriteLine(item.Path);
            }
        }
    }
}