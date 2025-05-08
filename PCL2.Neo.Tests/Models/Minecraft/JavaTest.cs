using PCL2.Neo.Models.Minecraft.Game.Data;
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
        }

        [Test]
        public void VersionCompare(){
            GameVersionNum v1;
        }
    }
}