using PCL.Neo.ViewModels;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public interface INavigationService
{
    Task<T> GotoAsync<T>() where T : ViewModelBase;
    Task<bool> GoBackAsync();
    bool CanGoBack { get; }
    ViewModelBase? CurrentViewModel { get; }
    ViewModelBase? CurrentSubViewModel { get; }
    event Action<ViewModelBase?>? CurrentViewModelChanged;
    event Action<ViewModelBase?>? CurrentSubViewModelChanged;
    event Action<NavigationEventArgs>? Navigating;
    void ClearHistory();
    void SetNavigationControl(Avalonia.Controls.UserControl control);
    // Add other methods from NavigationService that need to be exposed, for example:
    // bool CanGoBack { get; }
    // event Action<ViewModelBase?>? CurrentViewModelChanged;
    // event Action<ViewModelBase?>? CurrentSubViewModelChanged;
    // event Action<NavigationEventArgs>? Navigating;
    // void ClearHistory();
} 