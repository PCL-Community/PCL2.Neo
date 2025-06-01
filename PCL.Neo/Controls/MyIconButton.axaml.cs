using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using PCL.Neo.Helpers;
using PCL.Neo.Models;
using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Controls;

[PseudoClasses(":color", ":white", ":black", ":red", ":custom")]
public class MyIconButton : Button
{
    private Path? _pathIcon;
    private Border? _panBack;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _pathIcon = e.NameScope.Find<Path>("PathIcon")!;
        _panBack = e.NameScope.Find<Border>("PanBack")!;

        this.Loaded += (_, _) => RefreshColor();

        // 初始化
        _pathIcon.Data = Logo;
        _pathIcon.RenderTransform = new ScaleTransform { ScaleX = LogoScale, ScaleY = LogoScale };

        SetPseudoClass();
    }

    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }
        // TODO: remake animation system
        //await this.Animate().ScaleTo(0.8, durationMs: 400, easing: new QuadraticEaseOut()).RunAsync();
    }

    protected override async void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }
        // TODO: remake animation system
        //await this.Animate().ScaleTo(1d, durationMs: 250, easing: new MyBackEaseOut()).RunAsync();
    }

    public int Uuid = CoreUtils.GetUuid();

    public static readonly StyledProperty<Geometry> LogoProperty = AvaloniaProperty.Register<MyIconButton, Geometry>(
        nameof(Logo));

    public Geometry Logo
    {
        get => GetValue(LogoProperty);
        set
        {
            SetValue(LogoProperty, value);
            if (_pathIcon != null)
            {
                _pathIcon.Data = value;
            }
        }
    }

    public static readonly StyledProperty<double> LogoScaleProperty = AvaloniaProperty.Register<MyIconButton, double>(
        nameof(LogoScale),
        1);

    public double LogoScale
    {
        get => GetValue(LogoScaleProperty);
        set
        {
            SetValue(LogoScaleProperty, value);
            if (_pathIcon != null)
            {
                _pathIcon.RenderTransform = new ScaleTransform { ScaleX = value, ScaleY = value };
            }
        }
    }

    public enum IconThemes
    {
        Color,
        White,
        Black,
        Red,
        Custom
    }

    public static readonly StyledProperty<IconThemes> IconThemeProperty =
        AvaloniaProperty.Register<MyIconButton, IconThemes>(
            nameof(IconTheme),
            IconThemes.Color);

    public IconThemes IconTheme
    {
        get => GetValue(IconThemeProperty);
        set
        {
            SetValue(IconThemeProperty, value);
            SetPseudoClass();
        }
    }

    public new static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(Foreground));

    public new IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ForegroundInnerProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(ForegroundInner));

    public IBrush ForegroundInner
    {
        get => GetValue(ForegroundInnerProperty);
        set => SetValue(ForegroundInnerProperty, value);
    }

    public new static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<MyIconButton, IBrush>(
            nameof(Background));

    public new IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly StyledProperty<string> EventTypeProperty = AvaloniaProperty.Register<MyIconButton, string>(
        nameof(EventType));

    public string EventType
    {
        get => GetValue(EventTypeProperty);
        set => SetValue(EventTypeProperty, value);
    }

    public static readonly StyledProperty<string> EventDataProperty = AvaloniaProperty.Register<MyIconButton, string>(
        nameof(EventData));

    public string EventData
    {
        get => GetValue(EventDataProperty);
        set => SetValue(EventDataProperty, value);
    }

    /// <summary>
    /// 初始化颜色。
    /// </summary>
    private void RefreshColor()
    {
        if (_pathIcon is null || _panBack is null) return;
        _pathIcon.Fill = IconTheme switch
        {
            IconThemes.Color => (SolidColorBrush?)Application.Current!.Resources["ColorBrush5"],
            IconThemes.White => (SolidColorBrush)new MyColor(234, 242, 254),
            IconThemes.Red => (SolidColorBrush)new MyColor(160, 255, 76, 76),
            IconThemes.Black => (SolidColorBrush)new MyColor(160, 0, 0, 0),
            IconThemes.Custom => (SolidColorBrush)new MyColor(160, (SolidColorBrush)Foreground),
            _ => _pathIcon.Fill
        };

        _panBack.Background = (SolidColorBrush)new MyColor(0, 255, 255, 255);
    }

    private void SetPseudoClass()
    {
        switch (IconTheme)
        {
            case IconThemes.Color:
                PseudoClasses.Set(":color", true);
                break;

            case IconThemes.White:
                PseudoClasses.Set(":white", true);
                break;

            case IconThemes.Black:
                PseudoClasses.Set(":black", true);
                break;

            case IconThemes.Red:
                PseudoClasses.Set(":red", true);
                break;

            case IconThemes.Custom:
                PseudoClasses.Set(":custom", true);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}