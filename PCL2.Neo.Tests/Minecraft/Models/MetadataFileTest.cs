using PCL2.Neo.Minecraft.Models;
using System.Text.Json.Nodes;

namespace PCL2.Neo.Tests.Minecraft.Models;

public class MetadataFileTest
{
    [Test]
    public void Test()
    {
        foreach (var metadataFilePath in Directory.EnumerateFiles("./MCMetadataFiles"))
        {
            var jsonObj = JsonNode.Parse(File.ReadAllText(metadataFilePath))!.AsObject();
            var meta = new MetadataFile(jsonObj, false);
            meta.Parse();
            Assert.That(meta.Arguments.Game, Is.Not.Empty);
            if (jsonObj.ContainsKey("arguments"))
            {
                Assert.That(meta.Arguments.Game.Count, Is.EqualTo(jsonObj["arguments"]!["game"]!.AsArray().Count));
            }
            Assert.Multiple(() =>
            {
                Assert.That(meta.Assets, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Id, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Path, Is.Null);
                Assert.That(meta.AssetIndex.Sha1, Is.Not.Empty);
                Assert.That(meta.AssetIndex.Size, Is.Not.EqualTo(0));
                Assert.That(meta.AssetIndex.TotalSize, Is.Not.EqualTo(0));
            });
            Assert.That(meta.Downloads, Is.Not.Empty);
            foreach ((string id, MetadataFile.RemoteFileModel file) in meta.Downloads)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(id, Is.Not.Empty);
                    Assert.That(file.Path, Is.Null);
                    Assert.That(file.Sha1, Is.Not.Empty);
                    Assert.That(file.Size, Is.Not.EqualTo(0));
                    Assert.That(file.Url, Is.Not.Empty);
                });
            }
            Assert.That(meta.Id, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                if (meta.JavaVersion is null)
                    return;
                Assert.That(meta.JavaVersion.Component, Is.Not.Empty);
                Assert.That(meta.JavaVersion.MajorVersion, Is.Not.EqualTo(0));
            });
            Assert.That(meta.Libraries.Count, Is.EqualTo(jsonObj["libraries"]!.AsArray().Count));
            Assert.Multiple(() =>
            {
                if (meta.Logging is null)
                    return;
                Assert.That(meta.Logging, Is.Not.Empty);
                foreach ((string id, MetadataFile.LoggingModel logging) in meta.Logging)
                {
                    Assert.That(id, Is.Not.Empty);
                    Assert.That(logging.Argument, Is.Not.Empty);
                    Assert.That(logging.File, Is.Not.Null);
                    Assert.Multiple(() =>
                    {
                        Assert.That(logging.File.Id, Is.Not.Empty);
                        Assert.That(logging.File.Path, Is.Null);
                        Assert.That(logging.File.Sha1, Is.Not.Empty);
                        Assert.That(logging.File.Size, Is.Not.EqualTo(0));
                        Assert.That(logging.File.Url, Is.Not.Empty);
                    });
                    Assert.That(logging.Type, Is.Not.Empty);
                }
            });
            Assert.That(meta.MainClass, Is.Not.Empty);
            Assert.That(meta.MinimumLauncherVersion, Is.Not.EqualTo(0));
            Assert.That(meta.ReleaseTime, Is.Not.Empty);
            Assert.That(meta.Time, Is.Not.Empty);
            Assert.That(meta.Type, Is.Not.EqualTo(MetadataFile.ReleaseTypeEnum.Unknown));
        }
    }
}