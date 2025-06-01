using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;
using Avalonia.Media;
using PCL.Neo.Animations;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PCL.Neo.Helpers;

public static class AnimationHelper
{
    // TODO: Implement a way to cancel animations if needed, possibly using a dictionary to track active animations by control ID or similar.
    private static Dictionary<int, CancellationTokenSource> InAnimation { get; set; } = new();

    public static Animatable FadeTo(this Animatable control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(Visual.OpacityProperty);
        easing ??= new LinearEasing();
        var ani = new OpacityAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable FadeFromTo(this Animatable control, double begin, double target, uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        easing ??= new LinearEasing();
        var ani = new OpacityAnimation(new WeakReference<Animatable>(control), begin, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable ScaleTo(this Animatable control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var begX = control.GetValue(ScaleTransform.ScaleXProperty);
        var begY = control.GetValue(ScaleTransform.ScaleYProperty);
        var beg = new ScaleRate(begX, begY);

        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleAnimation(new WeakReference<Animatable>(control), beg,
            new ScaleRate(target, target), easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable ScaleXTo(this Animatable control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(ScaleTransform.ScaleXProperty);
        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleXAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration),
            TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }


    public static Animatable ScaleYTo(this Animatable control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(ScaleTransform.ScaleYProperty);
        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleYAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration),
            TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable RotateTo(this Animatable control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(RotateTransform.AngleProperty);
        easing ??= new LinearEasing();

        var ani = new RotateTransformAngleAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable RotateFromTo(this Animatable control, double begin, double end, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        easing ??= new LinearEasing();
        var ani = new RotateTransformAngleAnimation(new WeakReference<Animatable>(control), begin, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));
        var task = ani.RunAsync();
        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable TranslateTo(this Animatable control, Pos target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var begX = control.GetValue(TranslateTransform.XProperty);
        var begY = control.GetValue(TranslateTransform.YProperty);

        var beg = new Pos(begX, begY);

        easing ??= new LinearEasing();

        var ani = new TranslateTransformAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable TranslateXTo(this Animatable control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(TranslateTransform.XProperty);
        easing ??= new LinearEasing();
        var ani = new TranslateTransformXAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));
        var task = ani.RunAsync();
        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable TranslateYTo(this Animatable control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var beg = control.GetValue(TranslateTransform.YProperty);
        easing ??= new LinearEasing();
        var ani = new TranslateTransformYAnimation(new WeakReference<Animatable>(control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));
        var task = ani.RunAsync();
        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable MarginXTo(this Animatable control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var cot = (Layoutable)control;

        var beg = cot.Margin;
        var end = cot.HorizontalAlignment switch
        {
            HorizontalAlignment.Left =>
                new Thickness(cot.Margin.Left + target, cot.Margin.Top, cot.Margin.Right, cot.Margin.Bottom),
            HorizontalAlignment.Right =>
                new Thickness(cot.Margin.Left, cot.Margin.Top, cot.Margin.Right - target, cot.Margin.Bottom),
            _ => cot.Margin
        };

        easing ??= new LinearEasing();

        var ani = new XAnimation(new WeakReference<Animatable>(control), beg, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));
        var task = ani.RunAsync();
        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static Animatable MarginYTo(this Animatable control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var cot = (Layoutable)control;

        var beg = cot.Margin;
        var end = cot.VerticalAlignment switch
        {
            VerticalAlignment.Top => new Thickness(cot.Margin.Left, cot.Margin.Top + target,
                cot.Margin.Right, cot.Margin.Bottom),
            VerticalAlignment.Bottom => new Thickness(cot.Margin.Left, cot.Margin.Top, cot.Margin.Right,
                cot.Margin.Bottom - target),
            _ => cot.Margin
        };

        easing ??= new LinearEasing();

        var ani = new XAnimation(new WeakReference<Animatable>(control), beg, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay));
        var task = ani.RunAsync();
        if (wait)
        {
            task.Wait();
        }

        return control;
    }
}