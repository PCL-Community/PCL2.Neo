namespace PCL.Neo.Core.Models.Minecraft.Java;

public interface IJavaManager
{
    List<JavaRuntime> JavaList { get; }
    Task JavaListInit();
    Task<(JavaRuntime?, bool UpdateCurrent)> ManualAdd(string javaDir);
    Task Refresh();
}
