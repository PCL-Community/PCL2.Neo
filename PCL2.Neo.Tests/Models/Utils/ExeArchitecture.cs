using PCL2.Neo.Utils;

namespace PCL2.Neo.Tests.Models.Utils;

public class ExeArchitecture
{
    [Test]
    public void TestArchitecture()
    {
        string path = "/Users/amagicpear/Downloads/file_zip_win32_x64.exe";
        ExeArchitectureUtils.GetExecutableArchitecture(path);
    }
}