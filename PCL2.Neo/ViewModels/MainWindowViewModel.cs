using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Controls.MyMsg;
using PCL2.Neo.Services;

namespace PCL2.Neo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Window? _window;
        public NavigationService NavigationService { get; }

        [ObservableProperty]
        private ViewModelBase? _currentViewModel;

        // 为了设计时的DataContext
        public MainWindowViewModel()
        {
            throw new System.NotImplementedException();
        }
        public MainWindowViewModel(Window window)
        {
            this._window = window;
        }
        public MainWindowViewModel(NavigationService navigationService)
        {
            this.NavigationService = navigationService;
            this.NavigationService.CurrentViewModelChanged += x =>
            {
                CurrentViewModel = x;
            };
            this.NavigationService.Goto<HomeViewModel>();
        }

        [RelayCommand]
        public void NavigateToHome()
        {
            this.NavigationService.Goto<HomeViewModel>();
        }

        [RelayCommand]
        public void NavigateToDownload()
        {
            this.NavigationService.Goto<DownloadViewModel>();
        }

        public void Close()
        {
            _window?.Close();
        }

        public void Minimize()
        {
            if (_window is null) return;
            _window.WindowState = WindowState.Minimized;
        }

        public void ShowMessageBox(IMessageBox messageBox)
        {

        }

        /// <summary>
        /// 强制关闭正在窗口上展示的 MessageBox。
        /// </summary>
        public void CloseMessageBox()
        {

        }
    }
}
