using PCL2.Neo.Helpers;

namespace PCL2.Neo.Tests.Models.FileHelper;

public class FileTest
{
    [Test]
    public async Task Download()
    {
        await Helpers.FileHelper.DownloadFileAsync(new Uri(
                "https://piston-meta.mojang.com/v1/products/java-runtime/2ec0cc96c44e5a76b9c8b7c39df7210883d12871/all.json"),
            Path.Combine(Path.GetTempPath(), "all.json"));
        Console.WriteLine(Path.GetTempPath());
    }

    [Test]
    public async Task Fetch()
    {
        await Helpers.FileHelper.FetchJavaOnline("mac-os-arm64", "/Users/amagicpear/Downloads/PCL2Test",
            (completed, total) =>
            {
                Console.WriteLine($"下载进度：已下载{completed}/总文件数{total}");
            });
        // await Helpers.FileHelper.FetchJavaOnline("windows-x64",@"C:\Users\AMagicPear\Downloads\PCL2Test");
    }
}