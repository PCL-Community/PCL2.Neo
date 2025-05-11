using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using PCL.Neo.Controls;
using PCL.Neo.Helpers;
using PCL.Neo.Models.Minecraft.Java;
using PCL.Neo.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.Neo.Views.Home;

public partial class HomeSubView : UserControl
{
    public HomeSubView()
    {
        InitializeComponent();
    }

    private void Search_Java_Button(object? sender, RoutedEventArgs e)
    {
        // var testPath = Ioc.Default.GetService<StorageService>()?.SelectFile("Test");
        Task.Run(Ioc.Default.GetRequiredService<IJavaManager>().Refresh);
    }
}