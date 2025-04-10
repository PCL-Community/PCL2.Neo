using System;
using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace PCL2.Neo.Models;


public class MyColor
{
    public double A
    {
        get => _a;
        set => _a = Clamp(value, 0, 255);
    }
    private double _a = 255;


    public double R
    {
        get => _r;
        set => _r = Clamp(value, 0, 255);
    }
    private double _r;


    public double G
    {
        get => _g;
        set => _g = Clamp(value, 0, 255);
    }
    private double _g;


    public double B
    {
        get => _b;
        set => _b = Clamp(value, 0, 255);
    }
    private double _b;

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

    public static MyColor operator *(MyColor a, double b)
    {
        return new MyColor { A = a.A * b, B = a.B * b, G = a.G * b, R = a.R * b };
    }

    public static MyColor operator /(MyColor a, double b)
    {
        return new MyColor { A = a.A / b, B = a.B / b, G = a.G / b, R = a.R / b };
    }

    public static bool operator ==(MyColor a, MyColor b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
        if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
        return a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;
    }

    public static bool operator !=(MyColor a, MyColor b)
    {
        return !(a == b);
    }

    // 构造函数

    public MyColor()
    {
    }

    public MyColor(Color col)
    {
        A = col.A;
        R = col.R;
        G = col.G;
        B = col.B;
    }

    public MyColor(string hexString)
    {
        Color stringColor = Color.Parse(hexString);
        A = stringColor.A;
        R = stringColor.R;
        G = stringColor.G;
        B = stringColor.B;
    }


    public MyColor(double newA, MyColor col)
    {
        A = newA;
        R = col.R;
        G = col.G;
        B = col.B;
    }

    public MyColor(double newR, double newG, double newB)
    {
        A = 255;
        R = newR;
        G = newG;
        B = newB;
    }

    public MyColor(double newA, double newR, double newG, double newB)
    {
        A = newA;
        R = newR;
        G = newG;
        B = newB;
    }

    public MyColor(Brush brush)
    {
        SolidColorBrush solidBrush = (SolidColorBrush)brush;
        Color color = solidBrush.Color;
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    public MyColor(SolidColorBrush brush)
    {
        Color color = brush.Color;
        A = color.A;
        R = color.R;
        G = color.G;
        B = color.B;
    }

    // HSL转换

    public double Hue(double v1, double v2, double vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        if (vH < 0.16667) return v1 + (v2 - v1) * 6 * vH;
        if (vH < 0.5) return v2;
        if (vH < 0.66667) return v1 + (v2 - v1) * (4 - vH * 6);
        return v1;
    }

    public MyColor FromHsl(double sH, double sS, double sL)
    {
        if (sS == 0)
        {
            R = sL * 2.55;
            G = R;
            B = R;
        }
        else
        {
            double h = sH / 360;
            double s = sS / 100;
            double l = sL / 100;
            s = l < 0.5 ? s * l + l : s * (1.0 - l) + l;
            l = 2 * l - s;
            R = 255 * Hue(l, s, h + 1 / 3.0);
            G = 255 * Hue(l, s, h);
            B = 255 * Hue(l, s, h - 1 / 3.0);
        }
        A = 255;
        return this;
    }

    public MyColor FromHsl2(double sH, double sS, double sL)
    {
        if (sS == 0)
        {
            R = sL * 2.55;
            G = R;
            B = R;
        }
        else
        {
            sH = (sH + 3600000) % 360;
            double[] cent = {
                +0.1, -0.06, -0.3, // 0, 30, 60
                -0.19, -0.15, -0.24, // 90, 120, 150
                -0.32, -0.09, +0.18, // 180, 210, 240
                +0.05, -0.12, -0.02, // 270, 300, 330
                +0.1, -0.06}; // 最后两位与前两位一致，加是变亮，减是变暗
            double center = sH / 30.0;
            int intCenter = (int)Math.Floor(center);  // 亮度片区编号
            center = 50 - (
                (1 - center + intCenter) * cent[intCenter] + (center - intCenter) * cent[intCenter + 1]
            ) * sS;

            sL = sL < center ? sL / center : 1 + (sL - center) / (100 - center);
            sL *= 50;
            FromHsl(sH, sS, sL);
        }
        A = 255;
        return this;
    }

    public override string ToString()
    {
        return $"({A},{R},{G},{B})";
    }

    public override bool Equals(object obj)
    {
        return this == (MyColor)obj;
    }

    public static double Clamp(double value, double min, double max)
    {
        return Math.Min(Math.Max(value, min), max);
    }
}