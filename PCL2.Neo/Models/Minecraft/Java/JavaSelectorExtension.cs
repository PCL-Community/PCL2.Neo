using CommunityToolkit.Mvvm.DependencyInjection;
using PCL2.Neo.Models.Minecraft.Game.Data;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace PCL2.Neo.Models.Minecraft.Java;

public static class JavaSelectorExtension
{
    private static List<JavaRuntime>? JavaRuntimes { get { return Ioc.Default.GetService<IJavaManager>()?.JavaList; } }

    // public static ImmutableArray<JavaRuntime> SelectSuitableRuntimes(this GameEntityInfo gameEntityInfo)
    // {
    // }
}
