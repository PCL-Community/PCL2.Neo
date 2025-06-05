using Avalonia.Animation;
using System;
using System.Collections.Generic;
using System.Threading;
using IAnimation = PCL.Neo.Animations.IAnimation;

namespace PCL.Neo.Helpers.Animation
{
    public class AnimationChain(Animatable control) : IDisposable
    {
        internal Animatable Control { get; } = control;
        internal List<IAnimation> Animations { get; } = [];
        public bool IsComplete { get; internal set; }
        internal CancellationTokenSource CancellationToken { get; } = new();

        public void Cancel()
        {
            CancellationToken.Cancel();

            foreach (var animation in Animations)
            {
                animation.Cancel();
            }

            Animations.Clear();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CancellationToken.Dispose();
        }
    }
}