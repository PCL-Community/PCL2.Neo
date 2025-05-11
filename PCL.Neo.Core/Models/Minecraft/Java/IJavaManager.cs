namespace PCL.Neo.Core.Models.Minecraft.Java;

public interface IJavaManager
{
    List<JavaRuntime> JavaList { get; }
    string DefaultJavaPath { get; }
    Task JavaListInit();
    Task ManualAdd(string javaDir);
    Task Refresh();
}
