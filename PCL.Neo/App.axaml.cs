using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PCL.Neo.Services;
using Avalonia.Platform.Storage;
using PCL.Neo.Helpers;
using PCL.Neo.Utils;
using PCL.Neo.ViewModels;
using PCL.Neo.ViewModels.Download;
using PCL.Neo.ViewModels.Home;
using PCL.Neo.Views;
using System;
using System.Threading.Tasks;
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

            .AddTransient<HomeViewModel>()
            .AddTransient<HomeSubViewModel>()

            .AddTransient<DownloadViewModel>()
            .AddTransient<DownloadGameViewModel>()
            .AddTransient<DownloadModViewModel>()

            .AddTransient<SetupViewModel>()
            .AddTransient<SetupLaunchViewModel>()

            .AddSingleton<NavigationService>(s => new NavigationService(s))
            .AddSingleton<StorageService>()
            .BuildServiceProvider();

        public override void OnFrameworkInitializationCompleted()
        {
            Ioc.Default.ConfigureServices(ConfigureServices());

            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = vm
                };
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