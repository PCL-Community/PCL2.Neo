using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Download;

namespace PCL.Neo.ViewModels;

[DefaultSubViewModel(typeof(DownloadGameViewModel))]
public partial class DownloadViewModel(NavigationService navigationService) : ViewModelBase
{
    public NavigationService NavigationService { get; } = navigationService;

    [ObservableProperty] private string _message = "I am from DownloadViewModel";

    [RelayCommand]
    private void NavigateToDownloadGame()
    {
        this.NavigationService.Goto<DownloadGameViewModel>();
    }

    [RelayCommand]
    private void NavigateToDownloadMod()
    {
        this.NavigationService.Goto<DownloadModViewModel>();
    }

    [RelayCommand]
    private void Btn_Test1()
    {
        Message = "I am from DownloadViewModel Test1";
    }

    [RelayCommand]
    private void Btn_Test2()
    {
        Message = "I am from DownloadViewModel Test2";
    }
}