using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL2.Neo.ViewModels.Setup;

[SubViewModelOf(typeof(SetupViewModel))]
public partial class SetupLaunchViewModel : ViewModelBase
{
    private readonly IJavaManager _javaManager;
    [ObservableProperty] private List<JavaRuntime> _javaList = [];

    public SetupLaunchViewModel(IJavaManager javaManager)
    {
        _javaManager = javaManager;
        JavaList = _javaManager.JavaList;
    }

    [RelayCommand]
    private async Task RefreshJava()
    {
        await _javaManager.Refresh();
        JavaList = _javaManager.JavaList;
        Console.WriteLine($"UI显示列表有{JavaList.Count}个Java");
    }
}
