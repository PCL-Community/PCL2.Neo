using Avalonia.Animation;
using System.Collections.Generic;
using IAnimation = PCL.Neo.Animations.IAnimation;

namespace PCL.Neo.Helpers
{
    public class AnimationChain(Animatable control)
    {
        internal Animatable TargetControl { get; } = control;
        internal List<IAnimation> Animations { get; } = [];
        internal bool LoopChain { get; private set; }

        internal void AddAnimation(IAnimation animation)
        {
            Animations.Add(animation);
        }

        /// <summary>
        /// Sets whether the entire chain of animations should loop.
        /// </summary>
        public AnimationChain SetLoop(bool loop)
        {
            LoopChain = loop;
            return this;
        }
    }
}