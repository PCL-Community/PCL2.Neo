using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;

namespace PCL2.Neo.Animations;

public interface IAnimation
{
    /// <summary>
    /// 要动画的控件。
    /// </summary>
    Animatable Control { get; set; }
    /// <summary>
    /// 动画时间。
    /// </summary>
    TimeSpan Duration { get; set; }
    /// <summary>
    /// 延迟。
    /// </summary>
    TimeSpan Delay { get; set; }
    /// <summary>
    /// 缓动效果。
    /// </summary>
    Easing Easing { get; set; }
    /// <summary>
    /// 异步形式执行动画。
    /// </summary>
    Task RunAsync();
    /// <summary>
    /// 取消动画。
    /// </summary>
    void Cancel();
}