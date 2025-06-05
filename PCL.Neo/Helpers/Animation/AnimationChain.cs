using Avalonia.Animation;
using System;
using System.Collections.Generic;
using System.Threading;
using IAnimation = PCL.Neo.Animations.IAnimation;

namespace PCL.Neo.Helpers.Animation
{
    public class AnimationChain(Animatable control) : IAnimationChain, IDisposable
    {
        public Animatable Control { get; init; } = control;
        public List<IAnimation> Animations { get; } = [];
        public bool IsLoop { get; init; }
        public bool IsComplete { get; set; }
        public CancellationTokenSource CancellationToken { get; } = new();

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