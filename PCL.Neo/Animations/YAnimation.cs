using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Layout;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class YAnimation(
        Animatable control,
        double value,
        double? before = null,
        double? after = null,
        Easing? easing = null,
        TimeSpan? duration = null,
        TimeSpan? delay = null,
        bool wait = true)
        : BaseAnimation(control, before, after, easing, duration, delay, wait)
    {
        public double Value { get; set; } = value;

        /// <inheritdoc />
        public override Animation AnimationBuilder()
        {
            var control = (Layoutable)Control;
            var marginOriginal = control.Margin;
            var margin = control.VerticalAlignment switch
            {
                VerticalAlignment.Top => new Thickness(control.Margin.Left, control.Margin.Top + Value,
                    control.Margin.Right, control.Margin.Bottom),
                VerticalAlignment.Bottom => new Thickness(control.Margin.Left, control.Margin.Top, control.Margin.Right,
                    control.Margin.Bottom - Value),
                _ => control.Margin
            };

            return new Animation
            {
                Easing = Easing,
                Duration = Duration,
                Delay = Delay,
                FillMode = FillMode.Both,
                Children =
                {
                    new KeyFrame
                    {
                        Setters = { new Setter(Layoutable.MarginProperty, marginOriginal) }, Cue = new Cue(1d)
                    },
                    new KeyFrame { Setters = { new Setter(Layoutable.MarginProperty, margin) }, Cue = new Cue(1d) }
                }
            };
        }
    }
}