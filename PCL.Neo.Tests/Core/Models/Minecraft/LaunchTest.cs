using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Core.Utils;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Models.Minecraft
{
    public class LaunchTest
    {
        [Test]
        public async Task Test()
        {
            GameLauncher launcher = new(new GameService(new JavaManager()));
            LaunchOptions launchOptions = new();

            launchOptions.MinecraftDirectory = "/Users/yizhimcqiu/PCL-Mac-Minecraft";
            launchOptions.GameDirectory = "/Users/yizhimcqiu/PCL-Mac-Minecraft/versions/1.21.5";
            launchOptions.JavaPath = "/usr/bin/java";
            launchOptions.VersionId = "1.21.5";
            launchOptions.UUID = Uuid.GenerateOfflineUuid("MinecraftVenti");

            await launcher.LaunchAsync(launchOptions);
        }
    }
}