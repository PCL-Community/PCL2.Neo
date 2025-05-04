using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Services;
using PCL2.Neo.ViewModels.Download;

namespace PCL2.Neo.ViewModels.Home;

[SubViewModelOf(typeof(HomeViewModel))]
public partial class HomeSubViewModel : ViewModelBase
{
    public NavigationService NavigationService { get; }

    public HomeSubViewModel(NavigationService navigationService)
    {
        this.NavigationService = navigationService;
    }

    [RelayCommand]
    private void NavigateToDownloadMod()
    {
        this.NavigationService.Goto<DownloadModViewModel>();
    }
}