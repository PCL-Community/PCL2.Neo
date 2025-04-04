namespace PCL2.Neo.Models.Tests
{
    [TestClass]
    public class MainTests
    {
        [TestMethod]
        public void JavaSearchTest()
        {
            var result = Minecraft.Java.Windows.SearchJavaAsync(fullSearch: true).Result;
            foreach (var item in result)
            {
                Console.WriteLine(item.Path);
            }
        }
    }
}