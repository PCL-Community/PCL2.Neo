using Microsoft.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Media;
using PCL.Neo.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PCL.Neo.Animations.Easings;
using PCL.Neo.Animations;

namespace PCL.Neo.Services;

public enum NavigationType
{
    Forward,
    Backward
}

public class NavigationEventArgs : EventArgs
{
    public ViewModelBase? OldViewModel { get; }
    public ViewModelBase? NewViewModel { get; }
    public NavigationType NavigationType { get; }

    public NavigationEventArgs(ViewModelBase? oldViewModel, ViewModelBase? newViewModel, NavigationType navigationType)
    {
        OldViewModel = oldViewModel;
        NewViewModel = newViewModel;
        NavigationType = navigationType;
    }
}

public class NavigationService : INavigationService
{
    public IServiceProvider ServiceProvider { get; init; }

    public event Action<ViewModelBase?>? CurrentViewModelChanged;
    public event Action<ViewModelBase?>? CurrentSubViewModelChanged;
    public event Action<NavigationEventArgs>? Navigating;

    // 导航历史记录
    private readonly Stack<(Type ViewModelType, Type? SubViewModelType)> _navigationHistory = new();
    // 最大历史记录数量
    private const int MaxHistoryCount = 30;

    private ViewModelBase? _currentViewModel;
    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        protected set
        {
            if (value == _currentViewModel)
                return;
                
            var oldViewModel = _currentViewModel;
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
            
            var oldSubViewModel = _currentSubViewModel;
            _currentSubViewModel = value;
            CurrentSubViewModelChanged?.Invoke(value);
        }
    }

    public bool CanGoBack => _navigationHistory.Count > 0;
    
    // 页面导航控件引用
    private UserControl? _navigationControl;
    
    // 设置页面导航控件
    public void SetNavigationControl(UserControl control)
    {
        _navigationControl = control;
    }

    public NavigationService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    public async Task<T> GotoAsync<T>() where T : ViewModelBase
    {
        Type viewModelType = typeof(T);
        
        // 创建新的视图模型实例
        T newViewModel = ServiceProvider.GetRequiredService<T>();
        
        // 检查是否为子视图模型
        Type? mainViewModelType = null;
        SubViewModelOfAttribute? subViewModelOfAttribute = viewModelType.GetCustomAttribute<SubViewModelOfAttribute>();
        
        if (subViewModelOfAttribute != null)
        {
            mainViewModelType = subViewModelOfAttribute.MainViewModelType;
            
            // 如果是子视图模型，且主视图模型与当前不同，先导航到主视图模型
            if (CurrentViewModel?.GetType() != mainViewModelType)
            {
                // 创建主视图模型实例
                var mainViewModelInstance = ServiceProvider.GetRequiredService(mainViewModelType) as ViewModelBase;
                if (mainViewModelInstance == null)
                    throw new InvalidOperationException($"无法创建主视图模型实例: {mainViewModelType.Name}");
                
                // 保存当前视图模型到历史记录
                if (CurrentViewModel != null)
                {
                    _navigationHistory.Push((CurrentViewModel.GetType(), CurrentSubViewModel?.GetType()));
                    // 限制历史记录数量
                    if (_navigationHistory.Count > MaxHistoryCount)
                    {
                        var temp = new Stack<(Type, Type?)>();
                        for (int i = 0; i < MaxHistoryCount - 1; i++)
                        {
                            if (_navigationHistory.Count > 0)
                                temp.Push(_navigationHistory.Pop());
                        }
                        _navigationHistory.Clear();
                        while (temp.Count > 0)
                            _navigationHistory.Push(temp.Pop());
                    }
                }
                
                // 触发导航事件
                Navigating?.Invoke(new NavigationEventArgs(CurrentViewModel, mainViewModelInstance, NavigationType.Forward));
                
                // 设置当前视图模型
                CurrentViewModel = mainViewModelInstance;
            }
            
            // 设置子视图模型
            CurrentSubViewModel = newViewModel;
            
            // 应用导航动画
            await ApplyNavigationAnimation(NavigationType.Forward);
            
            return newViewModel;
        }
        else
        {
            // 如果不是子视图模型，直接设置当前视图模型
            if (CurrentViewModel != null)
            {
                _navigationHistory.Push((CurrentViewModel.GetType(), CurrentSubViewModel?.GetType()));
                // 限制历史记录数量
                if (_navigationHistory.Count > MaxHistoryCount)
                {
                    var temp = new Stack<(Type, Type?)>();
                    for (int i = 0; i < MaxHistoryCount - 1; i++)
                    {
                        if (_navigationHistory.Count > 0)
                            temp.Push(_navigationHistory.Pop());
                    }
                    _navigationHistory.Clear();
                    while (temp.Count > 0)
                        _navigationHistory.Push(temp.Pop());
                }
            }
            
            // 触发导航事件
            Navigating?.Invoke(new NavigationEventArgs(CurrentViewModel, newViewModel, NavigationType.Forward));
            
            // 设置当前视图模型和子视图模型
            CurrentViewModel = newViewModel;
            CurrentSubViewModel = null;
            
            // 应用导航动画
            await ApplyNavigationAnimation(NavigationType.Forward);
            
            return newViewModel;
        }
    }
    
    public async Task<bool> GoBackAsync()
    {
        if (!CanGoBack)
            return false;
            
        var (previousViewModelType, previousSubViewModelType) = _navigationHistory.Pop();
        
        // 创建前一个视图模型实例
        var previousViewModel = ServiceProvider.GetRequiredService(previousViewModelType) as ViewModelBase;
        if (previousViewModel == null)
            throw new InvalidOperationException($"无法创建视图模型实例: {previousViewModelType.Name}");
            
        // 触发导航事件
        Navigating?.Invoke(new NavigationEventArgs(CurrentViewModel, previousViewModel, NavigationType.Backward));
        
        // 设置当前视图模型
        CurrentViewModel = previousViewModel;
        
        // 如果有子视图模型，创建并设置
        if (previousSubViewModelType != null)
        {
            var previousSubViewModel = ServiceProvider.GetRequiredService(previousSubViewModelType) as ViewModelBase;
            if (previousSubViewModel == null)
                throw new InvalidOperationException($"无法创建子视图模型实例: {previousSubViewModelType.Name}");
                
            CurrentSubViewModel = previousSubViewModel;
        }
        else
        {
            CurrentSubViewModel = null;
        }
        
        // 应用导航动画
        await ApplyNavigationAnimation(NavigationType.Backward);
        
        return true;
    }
    
    // 清除导航历史记录
    public void ClearHistory()
    {
        _navigationHistory.Clear();
    }
    
    // 应用导航动画
    private async Task ApplyNavigationAnimation(NavigationType navigationType)
    {
        if (_navigationControl == null)
            return;
            
        try
        {
            // 导航动画在MainWindow中通过事件处理
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // 处理动画过程中的异常
            Console.WriteLine($"导航动画执行失败: {ex.Message}");
        }
    }
}