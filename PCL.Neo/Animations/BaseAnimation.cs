using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations
{
    public abstract class BaseAnimation(
        WeakReference<Animatable> control,
        double begin,
        double end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay)
        : IAnimation
    {
        /// <inheritdoc />
        public TimeSpan Delay { get; set; } = delay;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private WeakReference<Animatable> Control { get; } = control;
        protected TimeSpan Duration { get; } = duration;
        protected double? Begin { get; } = begin;
        protected double? End { get; } = end;
        protected Easing Easing { get; } = easing;

        /// <inheritdoc />
        public abstract Animation AnimationBuilder();

        /// <inheritdoc />
        public async Task RunAsync()
        {
            Control.TryGetTarget(out var target);
            await AnimationBuilder().RunAsync(target, _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}