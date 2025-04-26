using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PCL2.Neo.Animations;
using PCL2.Neo.Animations.Easings;
using PCL2.Neo.Controls;
using PCL2.Neo.Helpers;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    private async void Search_Java_Button(object? sender, RoutedEventArgs e)
    {
        try
        {
            var javas = await Java.SearchJava();
            Console.WriteLine($"找到 {javas.Count()} 个Java环境:");

            foreach (var java in javas)
            {
                try
                {
                    Console.WriteLine("----------------------");
                    Console.WriteLine($"路径: {java.DirectoryPath}");
                    var version = java.Version;
                    Console.WriteLine($"版本: Java {version}");
                    Console.WriteLine($"位数: {(java.Is64Bit ? "64位" : "32位")}");
                    Console.WriteLine($"类型: {(java.IsJre ? "JRE" : "JDK")}");
                    Console.WriteLine($"可用: {java.Compability}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理Java信息时出错: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"搜索失败: {ex.Message}");
        }
    }
}