using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using PCL.Neo.Animations;
using PCL.Neo.Animations.Easings;
using PCL.Neo.Controls;
using PCL.Neo.Helpers;
using PCL.Neo.Services;
using PCL.Neo.ViewModels;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CubicEaseOut = Avalonia.Animation.Easings.CubicEaseOut;

namespace PCL.Neo.Views;

public partial class MainWindow : Window
{
    private readonly CompositionVisual? _rightNavigationControlVisual;
    private readonly Compositor? _compositor;

    public MainWindow()
    {
        InitializeComponent();

        NavBackgroundBorder.PointerPressed += (i, e) =>
        {
            if (e.GetCurrentPoint(i as Control).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        };
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SetupSide("Left", StandardCursorType.LeftSide, WindowEdge.West);
            SetupSide("Right", StandardCursorType.RightSide, WindowEdge.East);
            SetupSide("Top", StandardCursorType.TopSide, WindowEdge.North);
            SetupSide("Bottom", StandardCursorType.BottomSide, WindowEdge.South);
            SetupSide("TopLeft", StandardCursorType.TopLeftCorner, WindowEdge.NorthWest);
            SetupSide("TopRight", StandardCursorType.TopRightCorner, WindowEdge.NorthEast);
            SetupSide("BottomLeft", StandardCursorType.BottomLeftCorner, WindowEdge.SouthWest);
            SetupSide("BottomRight", StandardCursorType.BottomRightCorner, WindowEdge.SouthEast);
        }

        new ThemeHelper(this).Refresh(Application.Current!.ActualThemeVariant);

        BtnTitleClose.Click += async (_, _) =>
        {
            AnimationOut();
            await Task.Delay(180);
            Close();
        };

        BtnTitleMin.Click += (_, _) => WindowState = WindowState.Minimized;
        
        // 设置导航按钮的事件处理
        SetupNavigationButtons();

        // 获取导航控件的CompositionVisual，用于动画
        _rightNavigationControlVisual = ElementComposition.GetElementVisual(RightNavigationControl);
        
        if (_rightNavigationControlVisual != null) 
        {
            _compositor = _rightNavigationControlVisual.Compositor;
            
            // 订阅导航事件
            this.Loaded += (_, _) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    // 使用接口方法，确保NavigationService被正确转换为INavigationService
                    INavigationService navigationService = viewModel.NavigationService;
                    
                    // 添加事件处理
                    navigationService.Navigating += OnNavigating;
                    
                    // 将导航控件引用传递给NavigationService
                    navigationService.SetNavigationControl(RightNavigationControl);
                }
            };
            
            this.Unloaded += (_, _) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    // 使用接口方法，确保NavigationService被正确转换为INavigationService
                    INavigationService navigationService = viewModel.NavigationService;
                    
                    // 移除事件处理
                    navigationService.Navigating -= OnNavigating;
                }
            };
        }

        GridRoot.Opacity = 0; // 在此处初始化透明度，不然将闪现
        this.Loaded += (_, _) => AnimationIn();
    }
    
    private void OnNavigating(NavigationEventArgs e)
    {
        if (_compositor == null || _rightNavigationControlVisual == null)
            return;
        
        // 导航动画效果
        if (e.NavigationType == NavigationType.Forward)
        {
            // 前进动画
            PlayForwardNavigationAnimation();
        }
        else
        {
            // 后退动画
            PlayBackwardNavigationAnimation();
        }
    }
    
    private void PlayForwardNavigationAnimation()
    {
        if (_compositor == null || _rightNavigationControlVisual == null)
            return;
        
        // 右侧面板动画 - 从右滑入
        var rightOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        rightOffsetAnimation.Duration = TimeSpan.FromMilliseconds(400);
        rightOffsetAnimation.InsertKeyFrame(0f, new Vector3(75f, 0f, 0f), new QuinticEaseInOut());
        rightOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new QuinticEaseInOut());
        rightOffsetAnimation.Target = "Offset";
        
        var rightOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        rightOpacityAnimation.Duration = TimeSpan.FromMilliseconds(400);
        rightOpacityAnimation.InsertKeyFrame(0f, 0f, new QuinticEaseInOut());
        rightOpacityAnimation.InsertKeyFrame(1f, 1f, new QuinticEaseInOut());
        rightOpacityAnimation.Target = "Opacity";
        
        // 应用动画
        var rightAnimationGroup = _compositor.CreateAnimationGroup();
        rightAnimationGroup.Add(rightOffsetAnimation);
        rightAnimationGroup.Add(rightOpacityAnimation);
        
        _rightNavigationControlVisual.StartAnimationGroup(rightAnimationGroup);
    }
    
    private void PlayBackwardNavigationAnimation()
    {
        if (_compositor == null || _rightNavigationControlVisual == null)
            return;
        
        // 右侧面板动画 - 从左滑入
        var rightOffsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
        rightOffsetAnimation.Duration = TimeSpan.FromMilliseconds(400);
        rightOffsetAnimation.InsertKeyFrame(0f, new Vector3(-75f, 0f, 0f), new QuinticEaseInOut());
        rightOffsetAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new QuinticEaseInOut());
        rightOffsetAnimation.Target = "Offset";
        
        var rightOpacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
        rightOpacityAnimation.Duration = TimeSpan.FromMilliseconds(400);
        rightOpacityAnimation.InsertKeyFrame(0f, 0f, new QuinticEaseInOut());
        rightOpacityAnimation.InsertKeyFrame(1f, 1f, new QuinticEaseInOut());
        rightOpacityAnimation.Target = "Opacity";
        
        // 应用动画
        var rightAnimationGroup = _compositor.CreateAnimationGroup();
        rightAnimationGroup.Add(rightOffsetAnimation);
        rightAnimationGroup.Add(rightOpacityAnimation);
        
        _rightNavigationControlVisual.StartAnimationGroup(rightAnimationGroup);
    }

    private void SetupSide(string name, StandardCursorType cursor, WindowEdge edge)
    {
        var ctl = this.Get<Control>(name);
        ctl.Cursor = new Cursor(cursor);
        ctl.PointerPressed += (i, e) =>
        {
            if (e.GetCurrentPoint(i as Control).Properties.IsLeftButtonPressed)
            {
                BeginResizeDrag(edge, e);
            }
        };
    }
    
    private async void AnimationIn()
    {
        GridRoot.Opacity = 1;
        MainBorder.Margin = new Thickness(18, 48, 18, 18);
        MainBorder.Opacity = 0;

        await Task.Delay(100);
        var border = MainBorder;
        var animation1 = new AnimationHelper(
        [
            new MarginAnimation(border, TimeSpan.FromMilliseconds(250), new Thickness(18), easing: new CubicEaseOut()),
            new OpacityAnimation(border, TimeSpan.FromMilliseconds(300), 0, 1, easing: new CubicEaseOut())
        ]);
        await animation1.RunAsync();
    }

    private void AnimationOut()
    {
        var animation = new AnimationHelper(
        [
            new MarginAnimation(MainBorder, TimeSpan.FromMilliseconds(150), new Thickness(18, 48, 18, 18),
                easing: new CubicEaseOut()),
            new OpacityAnimation(MainBorder, TimeSpan.FromMilliseconds(180), 1, 0, easing: new CubicEaseOut())
        ]);
        animation.Run();
    }

    private void SetupNavigationButtons()
    {
        // 获取按钮引用
        var btnLaunch = this.FindControl<Button>("NavBtnLaunch");
        var btnDownload = this.FindControl<Button>("NavBtnDownload");
        var btnSettings = this.FindControl<Button>("NavBtnSettings");
        var btnMore = this.FindControl<Button>("NavBtnMore");
        
        if (btnLaunch != null)
        {
            btnLaunch.Click += (s, e) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.LaunchGameCommand.Execute(null);
                }
            };
        }
        
        if (btnDownload != null)
        {
            btnDownload.Click += (s, e) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.NavigateToDownload();
                }
            };
        }
        
        if (btnSettings != null)
        {
            btnSettings.Click += (s, e) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.OpenSettingsCommand.Execute(null);
                }
            };
        }
        
        if (btnMore != null)
        {
            btnMore.Click += (s, e) => 
            {
                if (DataContext is MainWindowViewModel viewModel)
                {
                    viewModel.OpenMoreCommand.Execute(null);
                }
            };
        }
    }
}