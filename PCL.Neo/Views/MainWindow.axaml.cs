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
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CubicEaseOut = Avalonia.Animation.Easings.CubicEaseOut;

namespace PCL.Neo.Views;

public partial class MainWindow : Window
{
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

        LeftNavigationControl.Loaded += (_, _) =>
        {
            LeftNavigationControlBorder.Width = LeftNavigationControl!.Bounds.Width;

            LeftNavigationControl!.SizeChanged += (_, args) =>
            {
                if (args.WidthChanged)
                    LeftNavigationControlBorder.Width = args.NewSize.Width;
            };
        };

        GridRoot.Opacity = 0; // 在此处初始化透明度，不然将闪现
        this.Loaded += (_, _) => AnimationIn();
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
    /// <summary>
    /// 进入窗口的动画。
    /// </summary>
    private async void AnimationIn()
    {
        // var animation = new AnimationHelper(
        // [
        //     new OpacityAnimation(this, TimeSpan.FromMilliseconds(250), 0d, 1d),
        //     new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(600), 60d, 0d, new MyBackEaseOut(EasePower.Weak)),
        //     new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(500), -4d, 0d, new MyBackEaseOut(EasePower.Weak))
        // ]);
        // await animation.RunAsync();

        // AnimationHelper 性能太差，换用 CompositionAnimation
        await Task.Delay(100);

        var mainWindowCompositionVisual = ElementComposition.GetElementVisual(GridRoot)!;
        var compositor = mainWindowCompositionVisual.Compositor;

        var opacityFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityFrameAnimation.Duration = TimeSpan.FromMilliseconds(250);
        opacityFrameAnimation.InsertKeyFrame(0f, 0f, new CubicEaseOut());
        opacityFrameAnimation.InsertKeyFrame(1f, 1f, new CubicEaseOut());
        opacityFrameAnimation.Target = "Opacity";

        var rotateTransformAngleAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotateTransformAngleAnimation.Duration = TimeSpan.FromMilliseconds(500);
        rotateTransformAngleAnimation.InsertKeyFrame(0f, -0.06f, new MyBackEaseOut(EasePower.Weak));
        rotateTransformAngleAnimation.InsertKeyFrame(1f, 0f, new MyBackEaseOut(EasePower.Weak));
        rotateTransformAngleAnimation.Target = "RotationAngle";

        var translateTransformYAnimation = compositor.CreateVector3KeyFrameAnimation();
        translateTransformYAnimation.Duration = TimeSpan.FromMilliseconds(600);
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
    private void AnimationOut()
    {
        // var animation = new AnimationHelper(
        // [
        //     new OpacityAnimation(this, TimeSpan.FromMilliseconds(140), TimeSpan.FromMilliseconds(40), 0d, new QuadraticEaseOut()),
        //     new ScaleTransformScaleXAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
        //     new ScaleTransformScaleYAnimation(this, TimeSpan.FromMilliseconds(180), 0.88d),
        //     new TranslateTransformYAnimation(this, TimeSpan.FromMilliseconds(180), 20d, new QuadraticEaseOut()),
        //     new RotateTransformAngleAnimation(this, TimeSpan.FromMilliseconds(180), 0.6d, new QuadraticEaseInOut())
        // ]);
        // await animation.RunAsync();

        // AnimationHelper 性能太差，换用 CompositionAnimation

        var mainWindowCompositionVisual = ElementComposition.GetElementVisual(GridRoot)!;
        var compositor = mainWindowCompositionVisual.Compositor;

        var opacityFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityFrameAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
        opacityFrameAnimation.DelayTime = TimeSpan.FromMilliseconds(40);
        opacityFrameAnimation.Duration = TimeSpan.FromMilliseconds(140);
        opacityFrameAnimation.InsertKeyFrame(0f, 1f, new QuadraticEaseOut());
        opacityFrameAnimation.InsertKeyFrame(1f, 0f, new QuadraticEaseOut());
        opacityFrameAnimation.Target = "Opacity";

        var rotateTransformAngleAnimation = compositor.CreateScalarKeyFrameAnimation();
        rotateTransformAngleAnimation.Duration = TimeSpan.FromMilliseconds(180);
        rotateTransformAngleAnimation.InsertKeyFrame(0f, 0f, new QuadraticEaseOut());
        rotateTransformAngleAnimation.InsertKeyFrame(1f, 0.0006f, new QuadraticEaseOut());
        rotateTransformAngleAnimation.Target = "RotationAngle";

        var translateTransformYAnimation = compositor.CreateVector3KeyFrameAnimation();
        translateTransformYAnimation.Duration = TimeSpan.FromMilliseconds(180);
        translateTransformYAnimation.InsertKeyFrame(0f, new Vector3(0f, 0f, 0f), new QuadraticEaseOut());
        translateTransformYAnimation.InsertKeyFrame(1f, new Vector3(0f, 20f, 0f), new QuadraticEaseOut());
        translateTransformYAnimation.Target = "Offset";

        var scaleTransformAnimation = compositor.CreateVector3KeyFrameAnimation();
        scaleTransformAnimation.Duration = TimeSpan.FromMilliseconds(180);
        scaleTransformAnimation.InsertKeyFrame(0f, new Vector3(1f, 1f, 1f), new QuadraticEaseOut());
        scaleTransformAnimation.InsertKeyFrame(1f, new Vector3(0.88f, 0.88f, 1f), new QuadraticEaseOut());
        scaleTransformAnimation.Target = "Scale";

        var animationGroup = compositor.CreateAnimationGroup();
        animationGroup.Add(opacityFrameAnimation);
        animationGroup.Add(rotateTransformAngleAnimation);
        animationGroup.Add(translateTransformYAnimation);
        animationGroup.Add(scaleTransformAnimation);

        var size = mainWindowCompositionVisual.Size;
        mainWindowCompositionVisual.CenterPoint = new Vector3D((float)size.X / 2, (float)size.Y / 2, (float)mainWindowCompositionVisual.CenterPoint.Z);

        mainWindowCompositionVisual.StartAnimationGroup(animationGroup);
    }
}