using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    /// <summary>
    /// margin Animation
    /// </summary>
    /// <param name="begin">请自行添加 <see cref="GetCurrentMargin"/></param>
    public class MarginAnimation(
        Animatable control,
        double begin,
        double end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay,
        bool wait)
        : BaseAnimation(control, begin, end, easing, duration, delay, wait)
    {
        private static Thickness? GetCurrentMargin(Animatable control)
        {
            if (control is Control c)
            {
                return c.Margin;
            }
            return null;
        }

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
                    new KeyFrame { Setters = { new Setter(Layoutable.MarginProperty, Begin) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(Layoutable.MarginProperty, End) }, Cue = new Cue(1d) }
                }
            };
        }
    }
}