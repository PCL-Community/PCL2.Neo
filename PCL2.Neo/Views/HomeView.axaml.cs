using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PCL2.Neo.Controls;

namespace PCL2.Neo.Views;

public partial class HomeView : UserControl
{
    public HomeView()
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
}