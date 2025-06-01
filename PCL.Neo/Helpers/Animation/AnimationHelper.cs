using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;
using Avalonia.Media;
using PCL.Neo.Animations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL.Neo.Helpers.Animation;

public static class AnimationHelper
{
    public static AnimationChain Animate(this Animatable control) => new(control);

    public static AnimationChain LoopAnimate(this Animatable control) => new(control);

    public static async Task<AnimationChain> RunAsync(this AnimationChain chain)
    {
        do
        {
            foreach (var animation in chain.Animations)
            {
                if (chain.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var task = animation.RunAsync();
                if (animation.Wait)
                {
                    await task;
                }
            }
        } while (chain.CancellationToken.IsCancellationRequested == false && chain.IsLoop);


        chain.IsComplete = true;

        return chain;
    }

    public static async Task<bool> RUnAsync(IEnumerable<AnimationChain> chains)
    {
        foreach (var chain in chains)
        {
            await chain.RunAsync();
        }

        return true;
    }

    public static AnimationChain FadeTo(this AnimationChain control, double target, uint duration = 250, uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(Visual.OpacityProperty);
        easing ??= new LinearEasing();
        var ani = new OpacityAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);

        return control;
    }

    #region Fade

    public static AnimationChain FadeFromTo(this AnimationChain control, double begin, double target,
        uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        easing ??= new LinearEasing();
        var ani = new OpacityAnimation(new WeakReference<Animatable>(control.Control), begin, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);

        control.Animations.Add(ani);

        return control;
    }

    public static AnimationChain ScaleTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var begX = control.Control.GetValue(ScaleTransform.ScaleXProperty);
        var begY = control.Control.GetValue(ScaleTransform.ScaleYProperty);
        var beg = new ScaleRate(begX, begY);

        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleAnimation(new WeakReference<Animatable>(control.Control), beg,
            new ScaleRate(target, target), easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);

        control.Animations.Add(ani);
        return control;
    }

    #endregion

    #region Scale

    public static AnimationChain ScaleXTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(ScaleTransform.ScaleXProperty);
        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleXAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration),
            TimeSpan.FromMilliseconds(delay), wait);

        control.Animations.Add(ani);

        return control;
    }


    public static AnimationChain ScaleYTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(ScaleTransform.ScaleYProperty);
        easing ??= new LinearEasing();

        var ani = new ScaleTransformScaleYAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration),
            TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);

        return control;
    }

    #endregion

    #region Rotate

    public static AnimationChain RotateTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0,
        Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(RotateTransform.AngleProperty);
        easing ??= new LinearEasing();

        var ani = new RotateTransformAngleAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);

        var task = ani.RunAsync();

        if (wait)
        {
            task.Wait();
        }

        return control;
    }

    public static AnimationChain RotateFromTo(this AnimationChain control, double begin, double end,
        uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        easing ??= new LinearEasing();
        var ani = new RotateTransformAngleAnimation(new WeakReference<Animatable>(control.Control), begin, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);
        return control;
    }

    #endregion

    #region Translate

    public static AnimationChain TranslateTo(this AnimationChain control, Pos target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var begX = control.Control.GetValue(TranslateTransform.XProperty);
        var begY = control.Control.GetValue(TranslateTransform.YProperty);

        var beg = new Pos(begX, begY);

        easing ??= new LinearEasing();

        var ani = new TranslateTransformAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);

        control.Animations.Add(ani);
        return control;
    }

    public static AnimationChain TranslateXTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(TranslateTransform.XProperty);
        easing ??= new LinearEasing();
        var ani = new TranslateTransformXAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);
        return control;
    }

    public static AnimationChain TranslateYTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var beg = control.Control.GetValue(TranslateTransform.YProperty);
        easing ??= new LinearEasing();
        var ani = new TranslateTransformYAnimation(new WeakReference<Animatable>(control.Control), beg, target, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);
        return control;
    }

    #endregion

    #region Margin

    public static AnimationChain MarginXTo(this AnimationChain control, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var cot = (Layoutable)control.Control;

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

        var ani = new XAnimation(new WeakReference<Animatable>(control.Control), beg, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        control.Animations.Add(ani);
        return control;
    }

    public static AnimationChain MarginYTo(this AnimationChain contorl, double target, uint duration = 250,
        uint delay = 0, Easing? easing = null, bool wait = false)
    {
        var cot = (Layoutable)contorl.Control;

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

        var ani = new XAnimation(new WeakReference<Animatable>(contorl.Control), beg, end, easing,
            TimeSpan.FromMilliseconds(duration), TimeSpan.FromMilliseconds(delay), wait);
        contorl.Animations.Add(ani);
        return contorl;
    }

    #endregion
}