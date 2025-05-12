namespace PCL.Neo.Core.Models.Minecraft.Java;

public interface IJavaManager
{
    List<JavaRuntime> JavaList { get; }

    // string DefaultJavaPath { get; }
    (JavaRuntime Java8, JavaRuntime Java17, JavaRuntime Java21) DefaultJavaRuntime { get; }
    Task JavaListInit();
    Task ManualAdd(string javaDir);
    Task Refresh();
}