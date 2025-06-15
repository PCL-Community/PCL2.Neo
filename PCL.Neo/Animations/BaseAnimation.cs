using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations
{
    public abstract class BaseAnimation(
        Animatable control,
        double begin,
        double end,
        Easing easing,
        TimeSpan duration,
        TimeSpan delay,
        bool wait)
        : IAnimation
    {
        /// <inheritdoc />
        public TimeSpan Delay { get; } = delay;

        public bool Wait { get; } = wait;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Animatable Control { get; } = control;
        protected TimeSpan Duration { get; } = duration;
        protected double? Begin { get; } = begin;
        protected double? End { get; } = end;
        protected Easing Easing { get; } = easing;


        /// <inheritdoc />
        public abstract Animation AnimationBuilder();

        /// <inheritdoc />
        public async Task RunAsync()
        {
            await AnimationBuilder().RunAsync(Control, _cancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}