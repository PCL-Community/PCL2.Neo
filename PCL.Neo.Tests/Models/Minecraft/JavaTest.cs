using PCL.Neo.Models.Minecraft.Game.Data;
using PCL.Neo.Models.Minecraft.Java;
using PCL.Neo.Utils;

namespace PCL.Neo.Tests.Models.Minecraft
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