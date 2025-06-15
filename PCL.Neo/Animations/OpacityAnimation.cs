using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class OpacityAnimation(
        Animatable control,
        double begin,
        double end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay,
        bool wait)
        : BaseAnimation(control, begin, end, easing, duration, delay, wait)
    {
        /// <inheritdoc />
        public override Animation AnimationBuilder() =>
            new()
            {
                Easing = Easing,
                Duration = Duration,
                Delay = Delay,
                FillMode = FillMode.Both,
                Children =
                {
                    new KeyFrame { Setters = { new Setter(Visual.OpacityProperty, Begin) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(Visual.OpacityProperty, End) }, Cue = new Cue(1d) }
                }
            };
    }
}