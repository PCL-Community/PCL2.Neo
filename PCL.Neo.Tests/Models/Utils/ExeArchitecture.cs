using PCL.Neo.Utils;

namespace PCL.Neo.Tests.Models.Utils;

public class ExeArchitecture
{
    [Test]
    public void TestArchitecture()
    {
        string path = "/Users/amagicpear/Downloads/file_zip_win32_x64.exe";
        ExeArchitectureUtils.GetExecutableArchitecture(path);
    }
}