using PCL2.Neo.ViewModels;
using System;

namespace PCL2.Neo.Services;

public class NavigationService
{
    public event Action<ViewModelBase?>? CurrentViewModelChanged;
    protected readonly Func<Type, ViewModelBase> ViewModelFactory;

    private ViewModelBase? _currentViewModel;
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        protected set
        {
            if (value == _currentViewModel)
                return;
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke(value);
        }
    }

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        ViewModelFactory = viewModelFactory;
    }

    public virtual T Goto<T>() where T : ViewModelBase
    {
        var vm = ViewModelFactory(typeof(T));
        CurrentViewModel = vm;
        return (T)vm;
    }
}