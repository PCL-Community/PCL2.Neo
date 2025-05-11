using PCL.Neo.Core.Models.Minecraft.Java;

namespace PCL.Neo.Tests.Core.Models.Minecraft
{
    public class JavaTest
    {
        [Test]
        public async Task Test()
        {
            JavaManager javaInstance = new();
            await javaInstance.JavaListInit();
        }
    }
}