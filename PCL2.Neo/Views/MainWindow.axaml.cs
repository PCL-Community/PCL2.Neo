using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using PCL2.Neo.Animations;
using PCL2.Neo.Animations.Easings;
using PCL2.Neo.Controls;
using PCL2.Neo.Helpers;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Linq;
using System.Numerics;
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
        PART_RootGrid.Opacity = 0;//在此处初始化透明度，不然将闪现
    }
    private void OnNavPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        this.BeginMoveDrag(e);
    }
    /// <summary>
    /// 进入窗口的动画。
    /// </summary>
    private void AnimationIn()
    {
        // var animation = new AnimationHelper(
        // [
        //     new OpacityAnimation(this, TimeSpan.FromMilliseconds(250), 0d, 1d),
        //     new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(600), 60d, 0d, new MyBackEaseOut(EasePower.Weak)),
        //     new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(500), -4d, 0d, new MyBackEaseOut(EasePower.Weak))
        // ]);
        // await animation.RunAsync();

        // AnimationHelper 性能太差，换用 CompositionAnimation
        var mainWindowCompositionVisual = ElementComposition.GetElementVisual(PART_RootGrid)!;
        var compositor = mainWindowCompositionVisual.Compositor;

        var opacityFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityFrameAnimation.Duration = TimeSpan.FromSeconds(0.75);
        opacityFrameAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        opacityFrameAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        opacityFrameAnimation.Target = "Opacity";

        var rotateTransformAngleAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotateTransformAngleAnimation.Duration = TimeSpan.FromSeconds(0.75);
        rotateTransformAngleAnimation.InsertKeyFrame(0f, -0.06f, new MyBackEaseOut(EasePower.Weak));//
        rotateTransformAngleAnimation.InsertKeyFrame(1f, 0f, new MyBackEaseOut(EasePower.Weak));//
        rotateTransformAngleAnimation.Target = "RotationAngle";

        var translateTransformYAnimation = compositor.CreateVector3KeyFrameAnimation();
        translateTransformYAnimation.Duration = TimeSpan.FromSeconds(0.75);
        translateTransformYAnimation.InsertKeyFrame(0f, new Vector3(0f, 60f, 0f), new MyBackEaseOut(EasePower.Weak));
        translateTransformYAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 0f), new MyBackEaseOut(EasePower.Weak));
        translateTransformYAnimation.Target = "Offset";

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(opacityFrameAnimation);
        animationGroup.Add(rotateTransformAngleAnimation);
        animationGroup.Add(translateTransformYAnimation);

        var size = mainWindowCompositionVisual.Size;
        mainWindowCompositionVisual.CenterPoint = new Vector3D((float)size.X / 2, (float)size.Y / 2, (float)mainWindowCompositionVisual.CenterPoint.Z);

        mainWindowCompositionVisual.StartAnimationGroup(animationGroup);
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
        AnimationIn();
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
                Console.WriteLine("----------------------");
                Console.WriteLine($"路径: {java.Path}");
                Console.WriteLine($"版本: Java {java.Version}");
                Console.WriteLine($"位数: {(java.Is64Bit ? "64位" : "32位")}");
                Console.WriteLine($"类型: {(java.IsJre ? "JRE" : "JDK")}");
                Console.WriteLine($"可用: {java.IsUsable}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"搜索失败: {ex.Message}");
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AnimationIn();
    }
}