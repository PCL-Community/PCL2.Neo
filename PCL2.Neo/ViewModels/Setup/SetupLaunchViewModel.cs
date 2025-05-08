using CommunityToolkit.Mvvm.ComponentModel;
using PCL2.Neo.Models.Minecraft.Java;
using System.Collections.Generic;

namespace PCL2.Neo.ViewModels.Setup;

[SubViewModelOf(typeof(SetupViewModel))]
public partial class SetupLaunchViewModel : ViewModelBase
{
    [ObservableProperty] private List<JavaRuntime> _javaList = [];


}