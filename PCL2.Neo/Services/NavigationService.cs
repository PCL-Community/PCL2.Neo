using Microsoft.Extensions.DependencyInjection;
using PCL2.Neo.ViewModels;
using System;
using System.Reflection;

namespace PCL2.Neo.Services;

public class NavigationService
{
    public IServiceProvider ServiceProvider { get; init; }

    public event Action<ViewModelBase?>? CurrentViewModelChanged;
    public event Action<ViewModelBase?>? CurrentSubViewModelChanged;

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

    private ViewModelBase? _currentSubViewModel;
    public ViewModelBase? CurrentSubViewModel
    {
        get => _currentSubViewModel;
        protected set
        {
            if (value == _currentSubViewModel)
                return;
            _currentSubViewModel = value;
            CurrentSubViewModelChanged?.Invoke(value);
        }
    }

    public NavigationService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual T Goto<T>() where T : ViewModelBase
    {
        if (typeof(T).GetCustomAttribute<DefaultSubViewModelAttribute>() is { } dsvm)
        {
            var vm = CurrentViewModel as T;
            if (vm?.GetType() != typeof(T))
            {
                vm = ServiceProvider.GetRequiredService<T>();
                CurrentViewModel = vm;
            }
            if (CurrentSubViewModel?.GetType() != dsvm.SubViewModel)
                CurrentSubViewModel = ServiceProvider.GetRequiredService(dsvm.SubViewModel) as ViewModelBase;
            return vm;
        }

        if (typeof(T).GetCustomAttribute<SubViewModelOfAttribute>() is { } svmo)
        {
            var subVm = CurrentSubViewModel as T;
            if (CurrentViewModel?.GetType() != svmo.ParentViewModel)
                CurrentViewModel = ServiceProvider.GetRequiredService(svmo.ParentViewModel) as ViewModelBase;
            if (subVm?.GetType() != typeof(T))
            {
                subVm = ServiceProvider.GetRequiredService<T>();
                CurrentSubViewModel = subVm;
            }
            return subVm;
        }

        var targetVm = CurrentViewModel?.GetType() != typeof(T) || CurrentSubViewModel?.GetType() != typeof(T)
            ? ServiceProvider.GetRequiredService<T>()
            : (T)CurrentViewModel;
        if (CurrentViewModel?.GetType() != typeof(T))
            CurrentViewModel = targetVm;
        if (CurrentSubViewModel?.GetType() != typeof(T))
            CurrentSubViewModel = targetVm;
        return targetVm;
    }
}