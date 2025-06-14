using Avalonia.Controls;
using NUnit.Framework.Legacy;
using PCL.Neo.Core.Download;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Download
{
    [TestFixture]
    [TestOf(typeof(Downloader))]
    public class DownloadTest
    {
        [Test]
        public void DownloadTestAsync()
        {
            var downloader   = new Downloader([
                // new Downloader.DownloadTask("https://piston-meta.mojang.com/mc/game/version_manifest.json", @"C:\PCLNeoTest\version_manifest.json"),
                new Downloader.DownloadTask("https://piston-data.mojang.com/v1/objects/03f53214df599b9e39a560b94d0df5636858c32f/server.jar", @"C:\PCLNeoTest\server.jar"),
                // new Downloader.DownloadTask("https://speed.hetzner.de/100MB.bin", @"C:\PCLNeoTest\100M.bin")
            ], 1024);
            downloader.Run().Wait();
        }
    }
}