using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL2.Neo.Controls.MyMsg;
using PCL2.Neo.Services;
using PCL2.Neo.Helpers;
using System.Threading.Tasks;


namespace PCL2.Neo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Window? _window;
        public NavigationService NavigationService { get; }

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

        // 为了设计时的 DataContext
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
                switch (x)
                {
                    case HomeViewModel:
                        IsNavBtn1Checked = true;
                        break;
                    case DownloadViewModel:
                        IsNavBtn2Checked = true;
                        break;
                }
            };
            this.NavigationService.CurrentSubViewModelChanged += x =>
            {
                CurrentSubViewModel = x;
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

        public void ShowMessageBox((MessageBoxParam, TaskCompletionSource<MessageBoxResult>) messageBox)
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
