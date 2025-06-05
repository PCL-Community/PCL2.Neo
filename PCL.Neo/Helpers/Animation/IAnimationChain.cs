using Avalonia.Animation;
using System.Collections.Generic;
using System.Threading;
using IAnimation = PCL.Neo.Animations.IAnimation;

namespace PCL.Neo.Helpers.Animation
{
    public interface IAnimationChain
    {
        Animatable Control { get; init; }
        List<IAnimation> Animations { get; }
        bool IsLoop { get; init; }
        bool IsComplete { get;  set; }
        CancellationTokenSource CancellationToken { get; }

        void Cancel();
    }
}