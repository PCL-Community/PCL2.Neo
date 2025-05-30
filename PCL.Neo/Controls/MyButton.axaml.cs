using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using PCL.Neo.Animations;
using PCL.Neo.Helpers;
using PCL.Neo.Utils;
using System;

namespace PCL.Neo.Controls;

[Avalonia.Controls.Metadata.PseudoClasses(":normal", ":highlight", ":red")]
public class MyButton : Button
{
    private Border? _panFore;
    private readonly AnimationHelper _animation;

    public MyButton()
    {
        _animation = new AnimationHelper();
        Inlines = new InlineCollection();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _panFore = e.NameScope.Find<Border>("PanFore")!;

        if (Inlines!.Count == 0 && string.IsNullOrEmpty(Text))
        {
            Text = "Button";
        }

        SetPseudoClasses();
    }

    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _animation.CancelAndClear();
        _animation.Animations.AddRange([
            new ScaleTransformScaleAnimation(this, duration: TimeSpan.FromMilliseconds(80), after: 0.955d,
                easing: new CubicEaseOut())
        ]);
        await _animation.RunAsync();
    }

    protected override async void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        _animation.CancelAndClear();
        _animation.Animations.AddRange([
            new ScaleTransformScaleAnimation(this, duration: TimeSpan.FromMilliseconds(300), before: 0.955d, after: 1d,
                easing: new QuinticEaseOut(), wait: true)
        ]);
        await _animation.RunAsync();
    }

    public int Uuid = CoreUtils.GetUuid();

    public static readonly StyledProperty<InlineCollection?> InlinesProperty = AvaloniaProperty.Register<MyButton, InlineCollection?>(
        nameof(Inlines), new InlineCollection());

    [Content]
    public InlineCollection? Inlines
    {
        get => GetValue(InlinesProperty);
        set => SetValue(InlinesProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<Thickness> TextPaddingProperty = AvaloniaProperty.Register<MyButton, Thickness>(
        nameof(TextPadding));

    public Thickness TextPadding
    {
        get => GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    public enum ColorState
    {
        Normal,
        Highlight,
        Red
    }

    public static readonly StyledProperty<ColorState> ColorTypeProperty = AvaloniaProperty.Register<MyButton, ColorState>(
        nameof(ColorType));

    public ColorState ColorType
    {
        get => GetValue(ColorTypeProperty);
        set => SetValue(ColorTypeProperty, value);
    }

    public static readonly StyledProperty<Transform> RealRenderTransformProperty = AvaloniaProperty.Register<MyButton, Transform>(
        nameof(RealRenderTransform));

    public Transform RealRenderTransform
    {
        get => GetValue(RealRenderTransformProperty);
        set => SetValue(RealRenderTransformProperty, value);
    }

    public static readonly StyledProperty<string> EventTypeProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(EventType));

    public string EventType
    {
        get => GetValue(EventTypeProperty);
        set => SetValue(EventTypeProperty, value);
    }

    public static readonly StyledProperty<string> EventDataProperty = AvaloniaProperty.Register<MyButton, string>(
        nameof(EventData));

    public string EventData
    {
        get => GetValue(EventDataProperty);
        set => SetValue(EventDataProperty, value);
    }

    [Obsolete]
    private void RefreshColor()
    {
        if (_panFore is null) return;
        if (IsEnabled)
        {
            _panFore.BorderBrush = ColorType switch
            {
                ColorState.Normal => (IBrush?)Application.Current!.Resources["ColorBrush1"],
                ColorState.Highlight => (IBrush?)Application.Current!.Resources["ColorBrush2"],
                ColorState.Red => (IBrush?)Application.Current!.Resources["ColorBrushRedDark"],
                _ => _panFore.BorderBrush
            };
        }
        else
        {
            _panFore.BorderBrush = (SolidColorBrush)ThemeHelper.ColorGray4;
        }
        _panFore.Background = (IBrush?)Application.Current!.Resources["ColorBrushHalfWhite"];
    }

    private void SetPseudoClasses()
    {
        switch (ColorType)
        {
            case ColorState.Normal:
                PseudoClasses.Set(":normal", true);
                break;
            case ColorState.Highlight:
                PseudoClasses.Set(":highlight", true);
                break;
            case ColorState.Red:
                PseudoClasses.Set(":red", true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}