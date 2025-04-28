using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PCL2.Neo.Animations;
using PCL2.Neo.Animations.Easings;
using PCL2.Neo.Controls;
using PCL2.Neo.Helpers;
using PCL2.Neo.ViewModels;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using BounceEaseOut = Avalonia.Animation.Easings.BounceEaseOut;
using CubicEaseOut = Avalonia.Animation.Easings.CubicEaseOut;
using ExponentialEaseOut = Avalonia.Animation.Easings.ExponentialEaseOut;

namespace PCL2.Neo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        NavBackgroundBorder.PointerPressed += OnNavPointerPressed;

        new ThemeHelper(this).Refresh(Application.Current!.ActualThemeVariant);

        BtnTitleClose.Click += async (_, _) =>
        {
            await AnimationOut();
            Close();
        };

        BtnTitleMin.Click += (_, _) => WindowState = WindowState.Minimized;

        LeftNavigationControl.Loaded += (_, _) =>
        {
            LeftNavigationControlBorder.Width = LeftNavigationControl.Presenter!.Child?.Width ?? 0d;
            AnimationHelper? lastAnimation = null;
            LeftNavigationControl.Presenter!.PropertyChanged += async (_, e) =>
            {
                if (e.Property != ContentPresenter.ChildProperty)
                    return;
                var oldValue = e.OldValue as Control;
                var newValue = e.NewValue as Control;
                lastAnimation?.CancelAndClear();
                var previousScaleTransform =
                    (LeftNavigationControlBorder.RenderTransform as TransformGroup)?.Children
                    .FirstOrDefault(x => x is ScaleTransform) as ScaleTransform;
                var previousScaleX = previousScaleTransform?.ScaleX ?? 1d;
                LeftNavigationControlBorder.Width = LeftNavigationControl.Presenter!.Child?.Width ?? 0d;
                var scale = oldValue?.Width / newValue?.Width * previousScaleX ?? 1d;
                lastAnimation = new AnimationHelper(
                [
                    new ScaleTransformScaleXAnimation(LeftNavigationControlBorder, TimeSpan.FromMilliseconds(300), scale,
                        1d, new CubicEaseOut())
                ]);
                await lastAnimation.RunAsync();
            };
        };


        AnimationIn();
    }

    private void OnNavPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }
    /// <summary>
    /// 进入窗口的动画。
    /// </summary>
    private async void AnimationIn()
    {
        var animation = new AnimationHelper(
        [
            new OpacityAnimation(this, TimeSpan.FromMilliseconds(250), 0d, 1d),
            new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(600), 60d, 0d, new MyBackEaseOut(EasePower.Weak)),
            new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(500), -4d, 0d, new MyBackEaseOut(EasePower.Weak))
        ]);
        await animation.RunAsync();
    }
    /// <summary>
    /// 关闭窗口的动画。
    /// </summary>
    private async Task AnimationOut()
    {
        if (this.MainBorder.RenderTransform is null)
        {
            var animation = new AnimationHelper(
            [
                new OpacityAnimation(this, TimeSpan.FromMilliseconds(140), TimeSpan.FromMilliseconds(40), 0d, new QuadraticEaseOut()),
                new ScaleTransformScaleXAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
                new ScaleTransformScaleYAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
                new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(180), 20d, new QuadraticEaseOut()),
                new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(180), 0.6d, new QuadraticEaseInOut())
            ]);
            await animation.RunAsync();
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Loading;
    }

    private void Button2_OnClick(object? sender, RoutedEventArgs e)
    {
        this.TestLoading.State = MyLoading.LoadingState.Error;
    }
}