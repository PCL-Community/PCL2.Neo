using Avalonia.Animation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL.Neo.Helpers.Animation;

public static class AnimationHelper
{
    private static readonly ConcurrentDictionary<int, AnimationChain> InAnimationChains = new();

    public static AnimationChain Animate(this Animatable control)
    {
        var ani = new AnimationChain(control) { IsLoop = false };
        var hashCode = control.GetHashCode();

        // cancel and remove existing animation
        if (InAnimationChains.TryGetValue(hashCode, out var existingChain))
        {
            existingChain.Cancel();
            InAnimationChains.TryRemove(hashCode, out _);
        }

        InAnimationChains.TryAdd(hashCode, ani);

        return ani;
    }

    public static AnimationChain LoopAnimate(this Animatable control)
    {
        var ani = new AnimationChain(control) { IsLoop = true };
        var hashCode = control.GetHashCode();

        // cancel and remove existing animation
        if (InAnimationChains.TryGetValue(hashCode, out var existingChain))
        {
            existingChain.Cancel();
            InAnimationChains.TryRemove(hashCode, out _);
        }

        InAnimationChains.TryAdd(hashCode, ani);

        return ani;
    }

    private static async Task RunAnimation(AnimationChain chain)
    {
        var tasks = new List<Task>();
        do
        {
            foreach (var animation in chain.Animations)
            {
                if (chain.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var task = animation.RunAsync();
                tasks.Add(task);
                if (animation.Wait == false)
                {
                    continue;
                }

                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        } while (chain.CancellationToken.IsCancellationRequested == false && chain.IsLoop);

        chain.IsComplete = true;
    }

    public static async Task<AnimationChain> RunAsync(this AnimationChain chain)
    {
        await RunAnimation(chain);

        return chain;
    }
}