namespace PCL.Neo.Core.Models.Minecraft.Java;

using DefaultJavaRuntimeCombine = (JavaRuntime Java8, JavaRuntime Java17, JavaRuntime Java21);

public interface IJavaManager
{
    List<JavaRuntime> JavaList { get; }
    DefaultJavaRuntimeCombine DefaultJavaRuntimes { get; }
    Task JavaListInit();
    Task<(JavaRuntime?, bool UpdateCurrent)> ManualAdd(string javaDir);
    Task Refresh();
}