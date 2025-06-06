using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using PCL.Neo.Controls;
using PCL.Neo.Helpers;
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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        //this.TestLoading.State = MyLoading.LoadingState.Loading;
    }

    private void Button2_OnClick(object? sender, RoutedEventArgs e)
    {
        //this.TestLoading.State = MyLoading.LoadingState.Error;
    }
}