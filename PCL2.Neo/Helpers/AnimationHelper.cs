using PCL2.Neo.Animations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL2.Neo.Helpers;

/// <summary>
/// 动画帮助类，用来同时执行不同动画。
/// </summary>
public class AnimationHelper(List<IAnimation> animations)
{
    public List<IAnimation> Animations { get; set; } = animations;
    public List<Task> Tasks { get; } = new List<Task>();

    public AnimationHelper() : this([]){}

    public async Task RunAsync()
    {
        Tasks.Clear();
        foreach (IAnimation animation in Animations)
        {
            Tasks.Add(animation.RunAsync());
        }

        await Task.WhenAll(Tasks);
    }

    public void Cancel()
    {
        foreach (IAnimation animation in Animations)
        {
            animation.Cancel();
        }
    }

    public void CancelAndClear()
    {
        Cancel();
        Animations.Clear();
    }
}