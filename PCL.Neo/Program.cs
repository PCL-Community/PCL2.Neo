using System;
using Avalonia;
using Avalonia.Media;
using PCL.Neo.Services;

namespace PCL.Neo
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .With(new GameLauncher())
                .With(() => new FontManagerOptions
                {
                    FontFallbacks =
                    [
                        new() { FontFamily = "HarmonyOS Sans SC" },
                        new() { FontFamily = "鸿蒙黑体 SC" },
                        new() { FontFamily = ".AppleSystemUIFont" },
                        new() { FontFamily = "Microsoft YaHei UI" },
                        new() { FontFamily = "思源黑体 CN" },
                        new() { FontFamily = "Noto Sans CJK SC" }
                    ]
                });
    }
}
