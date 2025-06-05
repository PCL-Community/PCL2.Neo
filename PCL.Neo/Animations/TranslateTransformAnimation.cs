using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public record Pos(double X, double Y);

    public class TranslateTransformAnimation(
        Animatable control,
        Pos begin,
        Pos end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay,
        bool wait)
        : BaseAnimation(control, 0d, 0d, easing, duration, delay, wait)
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
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, begin.X),
                            new Setter(TranslateTransform.YProperty, begin.Y)
                        },
                        Cue = new Cue(0d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, end.X),
                            new Setter(TranslateTransform.YProperty, end.Y)
                        },
                        Cue = new Cue(1d)
                    }
                }
            };
    }
}
