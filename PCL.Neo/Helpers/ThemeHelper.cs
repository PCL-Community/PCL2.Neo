using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using PCL.Neo.Models;
using PCL.Neo.Views;
using System;

namespace PCL.Neo.Helpers;

public class ThemeHelper
{
    private readonly MainWindow _mainWindow;

    public static MyColor Color1 { get; } = new(52, 61, 74);
    public static MyColor Color2 { get; } = new(11, 91, 203);
    public static MyColor Color3 { get; } = new(19, 112, 243);
    public static MyColor Color4 { get; } = new(72, 144, 245);
    public static MyColor Color5 { get; } = new(150, 192, 249);
    public static MyColor Color6 { get; } = new(213, 230, 253);
    public static MyColor Color7 { get; } = new(222, 236, 253);
    public static MyColor Color8 { get; } = new(234, 242, 254);
    public static MyColor ColorBg0 { get; } = new(150, 192, 249);
    public static MyColor ColorBg1 { get; } = new(190, Color7);
    public static MyColor ColorGray1 { get; } = new(64, 64, 64);
    public static MyColor ColorGray2 { get; } = new(115, 115, 115);
    public static MyColor ColorGray3 { get; } = new(140, 140, 140);
    public static MyColor ColorGray4 { get; } = new(166, 166, 166);
    public static MyColor ColorGray5 { get; } = new(204, 204, 204);
    public static MyColor ColorGray6 { get; } = new(235, 235, 235);
    public static MyColor ColorGray7 { get; } = new(240, 240, 240);
    public static MyColor ColorGray8 { get; } = new(245, 245, 245);
    public static MyColor ColorSemiTransparent { get; } = new(1, Color8);

    private int _colorHue = 210, _colorSat = 85, _colorLightAdjust = 0, _colorHueTopbarDelta = 0;

    public ThemeHelper(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;

        Application.Current!.ActualThemeVariantChanged += (sender, _) =>
        {
            var themeVariant = ((IThemeVariantHost)sender!).ActualThemeVariant;
            Refresh(themeVariant);
        };
    }
    public void Refresh(ThemeVariant themeVariant)
    {
        // 标题栏
        var brushTitle = new LinearGradientBrush
        {
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative)
        };

        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 0,
            Color = MyColor.FromHsl2(_colorHue - _colorHueTopbarDelta, _colorSat, 48 + _colorLightAdjust)
        });
        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 0.5,
            Color = MyColor.FromHsl2(_colorHue, _colorSat, 54 + _colorLightAdjust)
        });
        brushTitle.GradientStops.Add(new GradientStop
        {
            Offset = 1,
            Color = MyColor.FromHsl2(_colorHue + _colorHueTopbarDelta, _colorSat, 48 + _colorLightAdjust)
        });

        _mainWindow.NavBackgroundBorder.Background = brushTitle;

        double lightAdjust = 1;
        if (themeVariant == ThemeVariant.Light)
        {
            lightAdjust = 1;
        }
        else if (themeVariant == ThemeVariant.Dark)
        {
            lightAdjust = 0.1;
        }

        // 背景
        var brushBackground = new LinearGradientBrush
        {
            EndPoint = new RelativePoint(0.1, 1, RelativeUnit.Relative),
            StartPoint = new RelativePoint(0.9, 0, RelativeUnit.Relative)
        };

        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = -0.1,
            Color = MyColor.FromHsl2(_colorHue - 20, Math.Min(60, _colorSat) * 0.5, 80 * lightAdjust)
        });
        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = 0.4,
            Color = MyColor.FromHsl2(_colorHue, _colorSat * 0.9, 90 * lightAdjust)
        });
        brushBackground.GradientStops.Add(new GradientStop
        {
            Offset = 1.1,
            Color = MyColor.FromHsl2(_colorHue + 20, Math.Min(60, _colorSat) * 0.5, 80 * lightAdjust)
        });

        _mainWindow.MainBorder.Background = brushBackground;
    }
}