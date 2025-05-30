using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class TranslateTransformYAnimation(
        Animatable control,
        double? before = null,
        double? after = null,
        Easing? easing = null,
        TimeSpan? duration = null,
        TimeSpan? delay = null)
        : BaseAnimation(control, before ?? control.GetValue(TranslateTransform.YProperty), after, easing, duration,
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
                    new KeyFrame { Setters = { new Setter(TranslateTransform.YProperty, Before) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(TranslateTransform.YProperty, After) }, Cue = new Cue(1d) }
                }
            };
    }
}