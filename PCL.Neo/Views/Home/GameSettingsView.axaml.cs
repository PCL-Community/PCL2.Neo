using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PCL.Neo.ViewModels.Home;

namespace PCL.Neo.Views.Home;

public partial class GameSettingsView : UserControl
{
    public GameSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
} 