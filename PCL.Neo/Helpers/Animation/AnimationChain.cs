using Avalonia.Animation;
using System.Collections.Generic;
using System.Threading;
using IAnimation = PCL.Neo.Animations.IAnimation;

namespace PCL.Neo.Helpers.Animation
{
    public class AnimationChain(Animatable control, bool loop = false)
    {
        internal Animatable Control { get; } = control;

        internal List<IAnimation> Animations { get; } = [];

        public bool IsLoop { get; } = loop;


        public bool IsComplete { get; internal set; } = false;
        public CancellationTokenSource CancellationToken { get; } = new();

        public void Cancel()
        {
            CancellationToken.Cancel();
        }

        public void Deconstruct(out Animatable Control)
        {
            Control = this.Control;
        }
    }
}