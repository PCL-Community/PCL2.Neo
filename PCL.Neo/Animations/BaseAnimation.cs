using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Animations
{
    public abstract class BaseAnimation(
        Animatable control,
        double? before = null,
        double? after = null,
        Easing? easing = null,
        TimeSpan? duration = null,
        TimeSpan? delay = null)
        : IAnimation
    {
        /// <inheritdoc />
        public TimeSpan Delay { get; set; } = delay ?? TimeSpan.Zero;

        /// <inheritdoc />
        public bool Wait { get; set; }

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        public Animatable Control { get; set; } = control;
        public TimeSpan Duration { get; set; } = duration ?? TimeSpan.FromSeconds(1);
        public double? Before { get; set; } = before;
        public double? After { get; set; } = after;
        public Easing Easing { get; set; } = easing ?? new LinearEasing();

        /// <summary>
        /// Build the animation
        /// </summary>
        /// <returns><see cref="Animation"/> The animation</returns>
        public abstract Animation AnimationBuilder();

        /// <inheritdoc />
        public async Task RunAsync() => await AnimationBuilder().RunAsync(Control, _cancellationTokenSource.Token);

        /// <inheritdoc />
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
