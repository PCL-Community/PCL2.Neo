using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PCL.Neo.Core.Models;
using PCL.Neo.Services;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.ViewModels;
using PCL.Neo.ViewModels.Download;
using PCL.Neo.ViewModels.Home;
using PCL.Neo.Views;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Java;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using PCL.Neo.ViewModels.Setup;

namespace PCL.Neo
{
    public partial class App : Application
    {
        // public static Java? JavaManager { get; private set; }
        // public static IStorageProvider StorageProvider { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private static IServiceProvider ConfigureServices() => new ServiceCollection()
            .AddTransient<MainWindowViewModel>()
            .AddTransient<ViewModels.Home.HomeViewModel>()
            .AddTransient<HomeSubViewModel>()
            .AddTransient<VersionManagerViewModel>()
            .AddTransient<GameSettingsViewModel>()
            .AddTransient<DownloadViewModel>()
            .AddTransient<DownloadGameViewModel>()
            .AddTransient<DownloadModViewModel>()
            .AddTransient<LogViewModel>()
            .AddTransient<SetupViewModel>()
            .AddTransient<SetupLaunchViewModel>()

            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<StorageService>()
            .AddSingleton<IJavaManager, JavaManager>()
            .AddSingleton<DownloadService>()
            .AddSingleton<GameService>()
            .AddSingleton<GameLauncher>(provider => new GameLauncher(provider.GetRequiredService<GameService>()))
            .AddSingleton<UserService>()
            .BuildServiceProvider();

        public override void OnFrameworkInitializationCompleted()
        {
            Ioc.Default.ConfigureServices(ConfigureServices());

            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            // JavaManager应该在GameService构造时自动初始化，而非在此调用初始化，故注释下面一行
            // Task.Run(Ioc.Default.GetRequiredService<IJavaManager>().JavaListInit);
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow { DataContext = vm };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}