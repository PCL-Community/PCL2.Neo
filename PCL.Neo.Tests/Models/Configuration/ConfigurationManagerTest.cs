using System.Text.Json;
using PCL.Neo.Core.Models.Configuration;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Models.Configuration
{
    [TestFixture]
    [TestOf(typeof(ConfigurationManager))]
    public class ConfigurationManagerTest
    {
        [ConfigurationInfo("testConfig.json")]
        public class TestConfiuration
        {
            public string Name  { get; set; } = "TestConfig";
            public int    Value { get; set; } = 42;
        }


        [Test]
        public async Task ConfigurationTest()
        {
            ConfigurationManager manager = new();
            var                  config  = new TestConfiuration();

            await manager.CreateCOnfiguration(config, null);
            var loadedConfig = manager.GetConfiguration<TestConfiuration>();
            var content = JsonSerializer.Serialize(loadedConfig, new JsonSerializerOptions() { WriteIndented = true });

            Console.WriteLine(content);

            var changed = loadedConfig.Name = "Hello Wrold!";
            await manager.UpdateConfiguration(loadedConfig, null);

            content = JsonSerializer.Serialize(loadedConfig, new JsonSerializerOptions() { WriteIndented = true });

            Console.WriteLine(content);
        }
    }
}