using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Services;
using PCL2.Neo.ViewModels.Download;

namespace PCL2.Neo.ViewModels;

[DefaultSubViewModel(typeof(DownloadGameViewModel))]
public partial class DownloadViewModel : ViewModelBase
{
    public NavigationService NavigationService { get; }

    [ObservableProperty]
    private string _message = "I am from DownloadViewModel";

    public DownloadViewModel(NavigationService navigationService)
    {
        NavigationService = navigationService;
    }

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