using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public record ScaleRate(double X, double Y);

    public class ScaleTransformScaleAnimation(
        WeakReference<Animatable> control,
        ScaleRate begin,
        ScaleRate end,
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
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, begin.X),
                            new Setter(ScaleTransform.ScaleYProperty, begin.Y)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, end.X),
                            new Setter(ScaleTransform.ScaleYProperty, end.Y)
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
        }
    }
}