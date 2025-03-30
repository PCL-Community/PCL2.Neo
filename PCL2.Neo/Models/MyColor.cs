using System;
using Avalonia.Media;
using System.Diagnostics.CodeAnalysis;
using Color = Avalonia.Media.Color;

namespace PCL2.Neo.Models;

public class MyColor : IEquatable<MyColor>
{
    public float A
    {
        get => _a;
        set => _a = Clamp(value, 0, 255);
    }

    private float _a = 255f;


    public float R
    {
        get => _r;
        set => _r = Clamp(value, 0, 255);
    }

    private float _r;


    public float G
    {
        get => _g;
        set => _g = Clamp(value, 0, 255);
    }

    private float _g;


    public float B
    {
        get => _b;
        set => _b = Clamp(value, 0, 255);
    }

    private float _b;

    // 类型转换
    public static implicit operator MyColor(string str) => new(str);

    public static implicit operator MyColor(Color col) => new(col);

    public static implicit operator Color(MyColor conv)
    {
        return Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255));
    }

    public static implicit operator System.Drawing.Color(MyColor conv)
    {
        return System.Drawing.Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255));
    }

    public static implicit operator MyColor(SolidColorBrush bru) => new(bru.Color);

    public static implicit operator SolidColorBrush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255)));
    }

    public static implicit operator MyColor(Brush bru) => new(bru);

    public static implicit operator Brush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255)));
    }

    // 颜色运算

    public static MyColor operator +(MyColor a, MyColor b) =>
        new() { A = a.A + b.A, B = a.B + b.B, G = a.G + b.G, R = a.R + b.R };


    public static MyColor operator -(MyColor a, MyColor b) =>
        new() { A = a.A - b.A, B = a.B - b.B, G = a.G - b.G, R = a.R - b.R };

    public static MyColor operator *(MyColor a, float b) =>
        new() { A = a.A * b, B = a.B * b, G = a.G * b, R = a.R * b };

    public static MyColor operator /(MyColor a, float b) =>
        new() { A = a.A / b, B = a.B / b, G = a.G / b, R = a.R / b };

    public static bool operator ==(MyColor a, MyColor b) => a.Equals(b);

    public static bool operator !=(MyColor a, MyColor b) => !(a == b);

    // 构造函数

    public MyColor()
    {
    }

    public MyColor(float newA, float newR, float newG, float newB)
    {
        A = newA;
        R = newR;
        G = newG;
        B = newB;
    }

    public MyColor(string hexString) : this(Color.Parse(hexString)) { }


    public MyColor(float newA, MyColor from) : this(newA, from.R, from.G, from.B) { }

    public MyColor(float newR, float newG, float newB) : this(255, newR, newG, newB) { }

    public MyColor(Color col) : this(col.A, col.R, col.G, col.B) { }

    public MyColor(Brush brush) : this(((SolidColorBrush)brush).Color) { }

    public MyColor(SolidColorBrush brush) : this(brush.Color) { }

    // HSL转换
    [Obsolete("This method is obsolete. Use ConvertHslToRgb instead.")]
    public static float Hue(float v1, float v2, float vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        return vH switch
        {
            < 0.16667f => v1 + (v2 - v1) * 6 * vH,
            < 0.5f => v2,
            < 0.66667f => v1 + (v2 - v1) * (4 - vH * 6),
            _ => v1
        };
    }

    public static float ConvertHslToRgb(float temp1, float temp2, float hue)
    {
        if (hue < 0.0f) hue += 1.0f;
        if (hue > 1.0f) hue -= 1.0f;

        return hue switch
        {
            < 1.0f / 6.0f => temp1 + (temp2 - temp1) * 6.0f * hue,
            < 1.0f / 2.0f => temp2,
            < 2.0f / 3.0f => temp1 + (temp2 - temp1) * (2.0f / 3.0f - hue) * 6.0f,
            _ => temp1
        };
    }

    public static MyColor FromHsl(MyColor myColor, float hue, float saturation, float lightness)
    {
        hue = Clamp(hue, 0, 360) % 360;
        saturation = Clamp(saturation, 0, 100);
        lightness = Clamp(lightness, 0, 100);

        if (Math.Abs(saturation) < 0.001f)
        {
            myColor.R = myColor.G = myColor.B = lightness / 100f;
        }
        else
        {
            var h = hue / 360.0f;
            var s = saturation / 100.0f;
            var l = lightness / 100.0f;

            var temp1 = l < 0.5f ? s * l + l : s * (1.0f - l) + l;
            var temp2 = 2 * l - s;

            myColor.R = ConvertHslToRgb(temp1, temp2, h + 1.0f / 3.0f);
            myColor.G = ConvertHslToRgb(temp1, temp2, h);
            myColor.B = ConvertHslToRgb(temp1, temp2, h - 1f / 3.0f);
        }

        myColor.A = 255;
        return myColor;
    }

    private static readonly float[] Cent =
    [
        +0.1f, -0.06f, -0.3f, // 0, 30, 60
        -0.19f, -0.15f, -0.24f, // 90, 120, 150
        -0.32f, -0.09f, +0.18f, // 180, 210, 240
        +0.05f, -0.12f, -0.02f, // 270, 300, 330
        +0.1f, -0.06f
    ]; // 最后两位与前两位一致，加是变亮，减是变暗

    public static MyColor FromHsl2(MyColor myColor, float sH, float sS, float sL)
    {
        if (sS == 0)
        {
            myColor.R = sL * 2.55f;
            myColor.G = myColor.R;
            myColor.B = myColor.R;
        }
        else
        {
            sH = (sH + 3600000) % 360;
            var center = sH / 30.0f;
            var intCenter = (int)Math.Floor(center); // 亮度片区编号
            center = 50 - (
                (1 - center + intCenter) * Cent[intCenter] + (center - intCenter) * Cent[intCenter + 1]
            ) * sS;

            sL = sL < center ? sL / center : 1 + (sL - center) / (100 - center);
            sL *= 50;
            FromHsl(myColor, sH, sS, sL);
        }

        myColor.A = 255;
        return myColor;
    }

    /// <summary>
    /// 将颜色转换为字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"({A},{R},{G},{B})";
    }

    /// <summary>
    /// 判断两个颜色是否相等
    /// </summary>
    /// <param name="other">需要判等的<see cref="MyColor"/></param>
    /// <returns>如果相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public bool Equals([NotNullWhen(true)] MyColor? other)
    {
#pragma warning disable CS8602 // 可能返回 null 引用。Have used NotNullWhen(true)
        if (ReferenceEquals(this, other))
            return true;

        return _a.Equals(other._a) && _r.Equals(other._r) && _g.Equals(other._g) && _b.Equals(other._b);
    }

    /// <summary>
    /// 判断是否相等
    /// </summary>
    /// <param name="obj">传入的Object<see cref="MyColor"/></param>
    /// <returns>如果相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((MyColor)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_a, _r, _g, _b);
    }

    /// <summary>
    /// 限制给定参数的范围
    /// </summary>
    /// <param name="value">要限制的参数</param>
    /// <param name="min">最大值</param>
    /// <param name="max">最小值</param>
    /// <returns>限制后的<see langword="float"/>值</returns>
    public static float Clamp(float value, float min, float max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}