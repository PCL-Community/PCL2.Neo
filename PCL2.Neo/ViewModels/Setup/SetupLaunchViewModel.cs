using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Models.Minecraft.Java;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PCL2.Neo.ViewModels.Setup;

public record JavaUiInfo(JavaRuntime Runtime)
{
    public string Identifier =>
        $"{(Runtime.IsJre ? "JRE" : "JDK")} {Runtime.SlugVersion} ({Runtime.Version}) {Runtime.Architecture}";

    public string Path => Runtime.DirectoryPath;
}

[SubViewModelOf(typeof(SetupViewModel))]
public partial class SetupLaunchViewModel : ViewModelBase
{
    private readonly IJavaManager _javaManager;
    [ObservableProperty] private ObservableCollection<JavaUiInfo> _javaInfoList = [];

    private void DoUiRefresh()
    {
        if (JavaInfoList.Count != 0) JavaInfoList.Clear();
        foreach (JavaRuntime runtime in _javaManager.JavaList)
            JavaInfoList.Add(new JavaUiInfo(runtime));
    }

    public SetupLaunchViewModel(IJavaManager javaManager)
    {
        _javaManager = javaManager;
        DoUiRefresh();
    }

    [RelayCommand]
    private async Task RefreshJava()
    {
        JavaInfoList.Clear();
        await _javaManager.Refresh();
        DoUiRefresh();
    }
}