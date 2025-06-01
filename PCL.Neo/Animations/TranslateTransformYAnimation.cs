using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class TranslateTransformYAnimation(
        WeakReference<Animatable> control,
        double begin,
        double end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay)
        : BaseAnimation(control,
            begin, end, easing, duration,
            delay)
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
                    new KeyFrame { Setters = { new Setter(TranslateTransform.YProperty, Begin) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(TranslateTransform.YProperty, End) }, Cue = new Cue(1d) }
                }
            };
    }
}