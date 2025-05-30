using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace PCL.Neo.Animations
{
    public class ScaleTransformScaleYAnimation(
        Animatable control,
        double? before = null,
        double? after = null,
        Easing? easing = null,
        TimeSpan? duration = null,
        TimeSpan? delay = null)
        : BaseAnimation(control, before ?? control.GetValue(ScaleTransform.ScaleYProperty), after, easing, duration,
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
                    new KeyFrame { Setters = { new Setter(ScaleTransform.ScaleYProperty, Before) }, Cue = new Cue(0d) },
                    new KeyFrame { Setters = { new Setter(ScaleTransform.ScaleYProperty, After) }, Cue = new Cue(1d) }
                }
            };
    }
}