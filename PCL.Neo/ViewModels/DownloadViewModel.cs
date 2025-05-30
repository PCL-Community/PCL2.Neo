using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Download;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels;

[DefaultSubViewModel(typeof(DownloadGameViewModel))]
public partial class DownloadViewModel(NavigationService navigationService) : ViewModelBase
{
    public NavigationService NavigationService { get; } = navigationService;

    [ObservableProperty] private string _message = "I am from DownloadViewModel";

    [RelayCommand]
    private async Task NavigateToDownloadGame()
    {
        await _navigationService.GotoAsync<DownloadGameViewModel>();
    }

    [RelayCommand]
    private async Task NavigateToDownloadMod()
    {
        await _navigationService.GotoAsync<DownloadModViewModel>();
    }

    [RelayCommand]
    private void Btn_Test1()
    {
        Message = "I am from DownloadViewModel             ੭ ˙ᗜ˙ )੭";
    }

    [RelayCommand]
    private void Btn_Test2()
    {
        Message = "I am from DownloadViewModel (⚭-⚭ ) ੭";
    }
}