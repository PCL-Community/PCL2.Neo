using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Controls.MyMsg;
using PCL.Neo.Services;
using PCL.Neo.Helpers;
using System.Threading.Tasks;


namespace PCL.Neo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Window? _window;
        public INavigationService NavigationService { get; }

        // quite shitty, maybe consider using enum and converters
        [ObservableProperty] private bool _isNavBtn1Checked = true;
        [ObservableProperty] private bool _isNavBtn2Checked;
        [ObservableProperty] private bool _isNavBtn3Checked;
        [ObservableProperty] private bool _isNavBtn4Checked;
        [ObservableProperty] private bool _isNavBtn5Checked;

        [ObservableProperty]
        private ViewModelBase? _currentViewModel;
        [ObservableProperty]
        private ViewModelBase? _currentSubViewModel;
        
        [ObservableProperty]
        private bool _canGoBack;

        // 添加新的属性和命令用于PCL II风格主界面
        [ObservableProperty] 
        private string _selectedGameVersion = "1.20.2-Fabric 0.15.7-OptiFine_I7_pre1";

        [ObservableProperty] 
        private bool _isPremiumAccount = false;
        
        // 添加CurrentUserName属性以解决绑定错误
        [ObservableProperty]
        private string _currentUserName = "Player";

        // 为了设计时的 DataContext
        public MainWindowViewModel()
        {
            throw new System.NotImplementedException();
        }
        public MainWindowViewModel(Window window)
        {
            this._window = window;
        }
        public MainWindowViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            NavigationService.CurrentViewModelChanged += vm =>
            {
                CurrentViewModel = vm;
                
                // 更新返回按钮状态
                CanGoBack = NavigationService.CanGoBack;
            };
            NavigationService.CurrentSubViewModelChanged += vm => CurrentSubViewModel = vm;
        }

        [RelayCommand]
        private void Close()
        {
            // 确保window不为空
            if (_window != null)
            {
                _window.Close();
            }
        }

        [RelayCommand]
        private void Minimize()
        {
            // 确保window不为空
            if (_window != null)
            {
                _window.WindowState = WindowState.Minimized;
            }
        }

        [RelayCommand]
        private void Maximize()
        {
            // 确保window不为空
            if (_window != null)
            {
                _window.WindowState = _window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }

        [RelayCommand]
        public async Task NavigateToHome()
        {
            await NavBtn1_ClickCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task NavigateToDownload()
        {
            await NavBtn2_ClickCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task NavBtn1_Click()
        {
            await NavigationService.GotoAsync<HomeViewModel>();
            ClearNavBtnState();
            IsNavBtn1Checked = true;
        }

        [RelayCommand]
        public async Task NavBtn2_Click()
        {
            await NavigationService.GotoAsync<DownloadViewModel>();
            ClearNavBtnState();
            IsNavBtn2Checked = true;
        }

        [RelayCommand]
        public async Task NavBtn3_Click()
        {
            // 导航到版本管理器
            await NavigationService.GotoAsync<PCL.Neo.ViewModels.Home.VersionManagerViewModel>();
            ClearNavBtnState();
            IsNavBtn3Checked = true;
        }

        [RelayCommand]
        public async Task NavBtn4_Click()
        {
            await NavigationService.GotoAsync<LogViewModel>();
            ClearNavBtnState();
            IsNavBtn4Checked = true;
        }

        [RelayCommand]
        public async Task NavBtn5_Click()
        {
            // 可以使用依赖注入或直接创建并使用SettingsViewModel
            // await NavigationService.GotoAsync<SettingsViewModel>();
            ClearNavBtnState();
            IsNavBtn5Checked = true;
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await NavigationService.GoBackAsync();
            // 更新导航按钮状态
            UpdateNavBtnState();
        }

        private void ClearNavBtnState()
        {
            IsNavBtn1Checked = false;
            IsNavBtn2Checked = false;
            IsNavBtn3Checked = false;
            IsNavBtn4Checked = false;
            IsNavBtn5Checked = false;
        }

        private void UpdateNavBtnState()
        {
            ClearNavBtnState();
            if (CurrentViewModel is HomeViewModel)
                IsNavBtn1Checked = true;
            else if (CurrentViewModel is DownloadViewModel)
                IsNavBtn2Checked = true;
            else if (CurrentViewModel is PCL.Neo.ViewModels.Home.VersionManagerViewModel)
                IsNavBtn3Checked = true;
            else if (CurrentViewModel is LogViewModel)
                IsNavBtn4Checked = true;
            // 还可以添加其他ViewModel类型的检查
        }

        public void ShowMessageBox((MessageBoxParam, TaskCompletionSource<MessageBoxResult>) messageBox)
        {

        }

        /// <summary>
        /// 强制关闭正在窗口上展示的 MessageBox。
        /// </summary>
        public void CloseMessageBox()
        {

        }

        [RelayCommand]
        private void SwitchToPremium()
        {
            IsPremiumAccount = true;
            // 可以在这里执行切换账户类型的逻辑
        }

        [RelayCommand]
        private void SwitchToOffline()
        {
            IsPremiumAccount = false;
            // 可以在这里执行切换账户类型的逻辑
        }

        [RelayCommand]
        private async Task GameSettings()
        {
            // 打开游戏设置页面
            // 假设HomeViewModel中已经有GameSettings命令的实现
            if (CurrentViewModel is HomeViewModel homeViewModel)
            {
                await homeViewModel.GameSettingsCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task ManageVersions()
        {
            // 打开版本管理页面
            // 假设HomeViewModel中已经有ManageVersions命令的实现
            if (CurrentViewModel is HomeViewModel homeViewModel)
            {
                await homeViewModel.ManageVersionsCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task LaunchGame()
        {
            // 启动游戏
            // 假设HomeViewModel中已经有LaunchGame命令的实现
            if (CurrentViewModel is HomeViewModel homeViewModel)
            {
                await homeViewModel.LaunchGameCommand.ExecuteAsync(null);
            }
        }

        // 打开设置页面
        [RelayCommand]
        private async Task OpenSettings()
        {
            // 打开设置页面
            // 实现打开设置页面的逻辑
        }

        // 打开更多选项
        [RelayCommand]
        private async Task OpenMore()
        {
            // 打开更多选项菜单
            // 实现打开更多选项的逻辑
        }
    }
}
