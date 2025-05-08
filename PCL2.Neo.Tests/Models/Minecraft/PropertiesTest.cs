using PCL2.Neo.Utils;

namespace PCL2.Neo.Tests.Models.Minecraft
{
    public class PropertiesTest
    {
        [Test]
        public void Test()
        {
            string version = PropertiesUtils.ReadProperties("/Library/Java/JavaVirtualMachines/zulu-24.jdk/Contents/Home/release")["JAVA_VERSION"];
            Console.WriteLine(version);
        }
    }
}