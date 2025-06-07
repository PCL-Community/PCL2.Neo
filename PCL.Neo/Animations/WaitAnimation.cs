using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations;

public class WaitAnimation(
    Animatable control,
    double target,
    Easing easing,
    TimeSpan duration,
    TimeSpan delay,
    bool wait) : BaseAnimation(control, 0d, target, easing, duration, delay, wait)
{
    /// <inheritdoc />
    public override Animation AnimationBuilder()
    {
        return new Animation
        {
            Delay = Delay,
            Duration = Duration,
            Children =
            {
                new KeyFrame { Cue = new Cue(0.0d), Setters = { new Setter(Visual.OpacityProperty, End) } }
            },
        };
    }
}