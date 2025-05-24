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

public interface INavigationService
{
    public event Action<NavigationEventArgs>? Navigating;
    public event Action<NavigationEventArgs>? Navigated;

    public ViewModelBase? CurrentMainViewModel { get; }

    public ViewModelBase? CurrentSubViewModel { get; }

    public bool CanGoBack { get; }

    public (ViewModelBase? mainVm, ViewModelBase? subVm) Goto<T>() where T : ViewModelBase;
    public (TM?, TS?) GoTo<TM, TS>() where TM : ViewModelBase where TS : ViewModelBase;

    public void NavigateTo(ViewModelBase? main, ViewModelBase? sub,
        NavigationType                    navigationType = NavigationType.Forward);

    public (ViewModelBase? mainVm, ViewModelBase? subVm) GoBack();

    public void ClearHistory();
}

public enum NavigationType
{
    Forward,
    Backward
}

public class NavigationEventArgs(
    ViewModelBase? oldMainViewModel,
    ViewModelBase? newMainViewModel,
    ViewModelBase? oldSubViewModel,
    ViewModelBase? newSubViewModel,
    NavigationType navigationType)
{
    public bool           IsMainViewModelChanged => oldMainViewModel != newMainViewModel;
    public ViewModelBase? OldMainViewModel       => oldMainViewModel;
    public ViewModelBase? NewMainViewModel       => newMainViewModel;
    public bool           IsSubViewModelChanged  => oldSubViewModel != newSubViewModel;
    public ViewModelBase? OldSubViewModel        => oldSubViewModel;
    public ViewModelBase? NewSubViewModel        => newSubViewModel;
    public NavigationType NavigationType         => navigationType;
}

public class NavigationService(IServiceProvider serviceProvider) : INavigationService
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public event Action<NavigationEventArgs>? Navigating;
    public event Action<NavigationEventArgs>? Navigated;

    // 导航历史记录
    private readonly LinkedList<(ViewModelBase?, ViewModelBase?)> _navigationHistory = [];

    // 最大历史记录数量
    private const int MaxHistoryCount = 30;

    public ViewModelBase? CurrentMainViewModel { get; protected set; }

    public ViewModelBase? CurrentSubViewModel { get; protected set; }

    public bool CanGoBack => _navigationHistory.Count > 0;

    /// <summary>
    /// 导航到类型为 T 的 MainViewModel 或 SubViewModel
    /// </summary>
    /// <typeparam name="T">要导航到的 ViewModel</typeparam>
    /// <returns>(MainViewModel, SubViewModel)</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public (ViewModelBase? mainVm, ViewModelBase? subVm) Goto<T>() where T : ViewModelBase
    {
        // T 可为 `MainViewModel` 或 `SubViewModel`
        // 根据 T 上附加的 attribute 判断
        var mainAttr = typeof(T).GetCustomAttribute<MainViewModelAttribute>();
        var subAttr  = typeof(T).GetCustomAttribute<SubViewModelAttribute>();
        if (mainAttr is null && subAttr is null)
            throw new InvalidOperationException(
                $"ViewModel {typeof(T).Name} does not have a {nameof(MainViewModelAttribute)} or a {nameof(SubViewModelAttribute)}");
        if (mainAttr is not null && subAttr is not null)
            throw new InvalidOperationException(
                $"ViewModel {typeof(T).Name} has both {nameof(MainViewModelAttribute)} and {nameof(SubViewModelAttribute)}");

        Type? mainVmType;
        Type? subVmType;
        if (mainAttr is not null)
        {
            mainVmType = typeof(T);
            subVmType  = mainAttr.DefaultSubViewModelType;
        }
        else // if (subAttr is not null)
        {
            mainVmType = subAttr!.MainViewModelType;
            subVmType  = typeof(T);
        }

        var mainVm = CurrentMainViewModel?.GetType() == mainVmType
            ? CurrentMainViewModel
            : (ViewModelBase)ServiceProvider.GetRequiredService(mainVmType);
        var subVm = CurrentSubViewModel?.GetType() == subVmType
            ? CurrentSubViewModel
            : (ViewModelBase)ServiceProvider.GetRequiredService(subVmType);

        NavigateTo(mainVm, subVm);

        return (mainVm, subVm);
    }

    /// <summary>
    /// 导航到类型为 TM 的 MainViewModel 与 类型为 TS 的 SubViewModel
    /// </summary>
    /// <typeparam name="TM">Type of MainViewModel</typeparam>
    /// <typeparam name="TS">Type of SubViewModel</typeparam>
    /// <returns>(MainViewModel, SubViewModel)</returns>
    public (TM?, TS?) GoTo<TM, TS>() where TM : ViewModelBase where TS : ViewModelBase
    {
        var mainVm = CurrentMainViewModel?.GetType() == typeof(TM)
            ? CurrentMainViewModel as TM
            : ServiceProvider.GetRequiredService<TM>();
        var subVm = CurrentSubViewModel?.GetType() == typeof(TS)
            ? CurrentSubViewModel as TS
            : ServiceProvider.GetRequiredService<TS>();

        NavigateTo(mainVm, subVm);

        return (mainVm, subVm);
    }

    /// <summary>
    /// 导航到 ViewModels
    /// </summary>
    /// <param name="main">MainViewModel</param>
    /// <param name="sub">SubViewModel</param>
    /// <param name="navigationType">导航方向</param>
    public void NavigateTo(ViewModelBase? main, ViewModelBase? sub,
        NavigationType                    navigationType = NavigationType.Forward)
    {
        var oldMainVm = CurrentMainViewModel;
        var oldSubVm = CurrentSubViewModel;

        Navigating?.Invoke(new NavigationEventArgs(
            oldMainVm, main,
            oldSubVm, sub,
            navigationType));
        PushHistory(main, sub);

        CurrentMainViewModel = main;
        CurrentSubViewModel  = sub;

        Navigated?.Invoke(new NavigationEventArgs(
            oldMainVm, main,
            oldSubVm, sub,
            navigationType));
    }

    /// <summary>
    /// 导航至上一级 并返回导航到的 ViewModels,
    /// 如没有上一级 则返回当前 ViewModels
    /// </summary>
    /// <returns>(MainViewModel, SubViewModel)</returns>
    public (ViewModelBase? mainVm, ViewModelBase? subVm) GoBack()
    {
        if (!TryPopHistory(out var main, out var sub))
            return (CurrentMainViewModel, CurrentSubViewModel);

        NavigateTo(main, sub, NavigationType.Backward);
        return (main, sub);
    }

    /// <summary>
    /// 清除导航历史
    /// </summary>
    public void ClearHistory() => _navigationHistory.Clear();

    /// <summary>
    /// 压入历史 ViewModels
    /// </summary>
    /// <param name="main"></param>
    /// <param name="sub"></param>
    private void PushHistory(ViewModelBase? main, ViewModelBase? sub)
    {
        _navigationHistory.AddFirst((main, sub));
        if (_navigationHistory.Count > MaxHistoryCount)
            _navigationHistory.RemoveLast();
    }

    /// <summary>
    /// 弹出历史 ViewModels
    /// </summary>
    /// <param name="main">MainViewModel</param>
    /// <param name="sub">SubViewModel</param>
    /// <returns>成功弹出</returns>
    private bool TryPopHistory(out ViewModelBase? main, out ViewModelBase? sub)
    {
        if (!CanGoBack)
        {
            main = null;
            sub  = null;
            return false;
        }

        var node = _navigationHistory.First!;
        main = node.Value.Item1;
        sub  = node.Value.Item2;
        _navigationHistory.RemoveFirst();
        return true;
    }
}