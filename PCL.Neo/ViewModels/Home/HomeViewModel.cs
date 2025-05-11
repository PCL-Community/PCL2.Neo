using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Services;
using PCL.Neo.ViewModels.Home;

namespace PCL.Neo.ViewModels.Home
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task ManageUsers()
        {
            // 导航到用户管理页面
            await _navigationService.GotoAsync<HomeSubViewModel>();
        }
        
        [RelayCommand]
        private async Task ManageVersions()
        {
            // 导航到版本管理页面
            await _navigationService.GotoAsync<VersionManagerViewModel>();
        }
    }
} 