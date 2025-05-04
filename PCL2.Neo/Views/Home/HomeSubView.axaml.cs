using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using PCL2.Neo.Controls;
using PCL2.Neo.Helpers;
using PCL2.Neo.Models.Minecraft.Java;
using PCL2.Neo.Services;
using System;
using System.Linq;

namespace PCL2.Neo.Views.Home;

public partial class HomeSubView : UserControl
{
    public HomeSubView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Loading;
    }

    private void Button2_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Error;
    }

    private async void Search_Java_Button(object? sender, RoutedEventArgs e)
    {
        // _ = App.JavaManager.Refresh();
        // IStorageProvider test = TopLevel.GetTopLevel(this)!.StorageProvider;
        var testPath = Ioc.Default.GetService<StorageService>()?.SelectFile("Test");
    }
}