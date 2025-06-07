using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class XAnimation(
        Animatable control,
        Thickness begin,
        Thickness end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay,
        bool wait)
        : BaseAnimation(control, 0d, 0d, easing, duration, delay, wait)
    {
        /// <inheritdoc />
        public override Animation AnimationBuilder()
        {
            return new Animation
            {
                Easing = Easing,
                Duration = Duration,
                Delay = Delay,
                FillMode = FillMode.Both,
                Children =
                {
                    new KeyFrame { Setters = { new Setter(Layoutable.MarginProperty, begin) }, Cue = new Cue(1d) },
                    new KeyFrame { Setters = { new Setter(Layoutable.MarginProperty, end) }, Cue = new Cue(1d) }
                }
            };
        }
    }
}