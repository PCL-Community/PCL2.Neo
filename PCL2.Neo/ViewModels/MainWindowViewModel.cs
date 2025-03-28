using Avalonia.Controls;
using PCL2.Neo.Controls.MyMsg;

namespace PCL2.Neo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Window? _window;

        // 为了设计时的DataContext
        public MainWindowViewModel()
        {
        }
        public MainWindowViewModel(Window window)
        {
            this._window = window;
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
