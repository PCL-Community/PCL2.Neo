using PCL2.Neo.Utils;

namespace PCL2.Neo.Tests.Utils
{
    public class PeHeaderReaderTest
    {
        [Test]
        public void GetMachineTest()
        {
            // Arrange
            const string
                path =
                    @"C:\Users\WhiteCAT\Desktop\tools\Twitch Drops Miner\Twitch Drops Miner (by DevilXD).exe"; // Replace with your test file path
            const ushort expectedMachine = 0x8664; // Replace with the expected machine type
            // Act
            ushort actualMachine = PeHeaderReader.GetMachine(path);
            // Assert
            Assert.AreEqual(expectedMachine, actualMachine);
        }
    }
}
