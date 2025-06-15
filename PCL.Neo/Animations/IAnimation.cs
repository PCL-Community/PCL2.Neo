using Avalonia.Animation;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public interface IAnimation
{
    /// <summary>
    /// 延迟。
    /// </summary>
    TimeSpan Delay { get; }

    bool Wait { get; }

    /// <summary>
    /// Build the animation
    /// </summary>
    /// <returns><see cref="Animation"/> The animation</returns>
    public Animation AnimationBuilder();

    /// <summary>
    /// 异步形式执行动画。
    /// </summary>
    Task RunAsync();

    /// <summary>
    /// 取消动画。
    /// </summary>
    void Cancel();
}