using System;
using Avalonia.Media;
using System.Diagnostics.CodeAnalysis;
using Color = Avalonia.Media.Color;

namespace PCL2.Neo.Models;

public class MyColor
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
    public static implicit operator MyColor(string str)
    {
        return new MyColor(str);
    }

    public static implicit operator MyColor(Color col)
    {
        return new MyColor(col);
    }

    public static implicit operator Color(MyColor conv)
    {
        return Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255));
    }

    public static implicit operator System.Drawing.Color(MyColor conv)
    {
        return System.Drawing.Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255));
    }

    public static implicit operator MyColor(SolidColorBrush bru)
    {
        return new MyColor(bru.Color);
    }

    public static implicit operator SolidColorBrush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255)));
    }

    public static implicit operator MyColor(Brush bru)
    {
        return new MyColor(bru);
    }

    public static implicit operator Brush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Clamp(conv.A, 0, 255), (byte)Clamp(conv.R, 0, 255), (byte)Clamp(conv.G, 0, 255), (byte)Clamp(conv.B, 0, 255)));
    }

    // 颜色运算

    public static MyColor operator +(MyColor a, MyColor b)
    {
        return new MyColor { A = a.A + b.A, B = a.B + b.B, G = a.G + b.G, R = a.R + b.R };
    }


    public static MyColor operator -(MyColor a, MyColor b)
    {
        return new MyColor { A = a.A - b.A, B = a.B - b.B, G = a.G - b.G, R = a.R - b.R };
    }

    public static MyColor operator *(MyColor a, float b)
    {
        return new MyColor { A = a.A * b, B = a.B * b, G = a.G * b, R = a.R * b };
    }

    public static MyColor operator /(MyColor a, float b)
    {
        return new MyColor { A = a.A / b, B = a.B / b, G = a.G / b, R = a.R / b };
    }

    public static bool operator ==(MyColor a, MyColor b) => a.Equals(b);

    public static bool operator !=(MyColor a, MyColor b)
    {
        return !(a == b);
    }

    // 构造函数

    public MyColor()
    {
    }

    public MyColor(string hexString)
    {
        Color stringColor = Color.Parse(hexString);
        A = stringColor.A;
        R = stringColor.R;
        G = stringColor.G;
        B = stringColor.B;
    }


    public MyColor(float newA, MyColor col)
    {
        A = newA;
        R = col.R;
        G = col.G;
        B = col.B;
    }

    public MyColor(float newR, float newG, float newB)
    {
        A = 255;
        R = newR;
        G = newG;
        B = newB;
    }

    public MyColor(float newA, float newR, float newG, float newB)
    {
        A = newA;
        R = newR;
        G = newG;
        B = newB;
    }

    public MyColor(Color col)
    {
        A = col.A;
        R = col.R;
        G = col.G;
        B = col.B;
    }

    public MyColor(Brush brush)
    {
        var solidBrush = (SolidColorBrush)brush;
        var color = solidBrush.Color;
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public MyColor(SolidColorBrush brush)
    {
        var color = brush.Color;
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    // HSL转换

    public float Hue(float v1, float v2, float vH)
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

    public MyColor FromHsl(float sH, float sS, float sL)
    {
        if (sS == 0)
        {
            R = sL * 2.55f;
            G = R;
            B = R;
        }
        else
        {
            float h = sH / 360f;
            float s = sS / 100f;
            float l = sL / 100f;
            s = l < 0.5f ? s * l + l : s * (1.0f - l) + l;
            l = 2 * l - s;
            R = 255f * Hue(l, s, h + 1f / 3.0f);
            G = 255f * Hue(l, s, h);
            B = 255f * Hue(l, s, h - 1f / 3.0f);
        }
        A = 255;
        return this;
    }

    private static readonly float[] Cent =
    [
        +0.1f, -0.06f, -0.3f, // 0, 30, 60
        -0.19f, -0.15f, -0.24f, // 90, 120, 150
        -0.32f, -0.09f, +0.18f, // 180, 210, 240
        +0.05f, -0.12f, -0.02f, // 270, 300, 330
        +0.1f, -0.06f
    ]; // 最后两位与前两位一致，加是变亮，减是变暗

    public MyColor FromHsl2(float sH, float sS, float sL)
    {
        if (sS == 0)
        {
            R = sL * 2.55f;
            G = R;
            B = R;
        }
        else
        {
            sH = (sH + 3600000) % 360;
            float center = sH / 30.0f;
            int intCenter = (int)Math.Floor(center);  // 亮度片区编号
            center = 50 - (
                (1 - center + intCenter) * Cent[intCenter] + (center - intCenter) * Cent[intCenter + 1]
            ) * sS;

            sL = sL < center ? sL / center : 1 + (sL - center) / (100 - center);
            sL *= 50;
            FromHsl(sH, sS, sL);
        }
        A = 255;
        return this;
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
    /// 判等
    /// </summary>
    /// <param name="other">需要判等的<see cref="MyColor"/></param>
    /// <returns>如果相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public bool Equals(MyColor other)
    {
        if (ReferenceEquals(this, other))
            return true;

        return _a.Equals(other._a) && _r.Equals(other._r) && _g.Equals(other._g) && _b.Equals(other._b);
    }

    /// <summary>
    /// 判等
    /// </summary>
    /// <param name="fir">需要判等的<see cref="MyColor"/></param>
    /// /// <param name="sec">需要判等的<see cref="MyColor"/></param>
    /// <returns>如果相等返回<see langword="true"/>，否则返回<see langword="false"/></returns>
    public static bool Equals(MyColor fir, MyColor sec) => fir.Equals(sec);

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