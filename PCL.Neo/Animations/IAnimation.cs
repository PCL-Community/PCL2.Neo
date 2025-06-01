using Avalonia.Animation;
using System;
using System.Threading.Tasks;

namespace PCL.Neo.Animations;

public interface IAnimation
{
    /// <summary>
    /// 延迟。
    /// </summary>
    TimeSpan Delay { get; set; }

    /// <summary>
    /// Build the animation
    /// </summary>
    /// <returns><see cref="Animation"/> The animation</returns>
    public abstract Animation AnimationBuilder();

    /// <summary>
    /// 异步形式执行动画。
    /// </summary>
    Task RunAsync();

    /// <summary>
    /// 取消动画。
    /// </summary>
    void Cancel();
}