using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class OpacityAnimation(
        Animatable control,
        double? before = null,
        double? after = null,
        Easing? easing = null,
        TimeSpan? duration = null,
        TimeSpan? delay = null,
        bool wait = true)
        : BaseAnimation(control, before ?? control.GetValue(Visual.OpacityProperty), after, easing, duration,
            delay, wait)
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
                    new KeyFrame { Setters = { new Setter(Visual.OpacityProperty, Before) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(Visual.OpacityProperty, After) }, Cue = new Cue(1d) }
                }
            };
    }
}