using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using System;

namespace PCL2.Neo.Animations;

public interface IAnimation
{
    /// <summary>
    /// 延迟。
    /// </summary>
    TimeSpan Delay { get; set; }
    /// <summary>
    /// 指示动画是否要等待上一个动画完成后再执行。与 AnimationHelper 搭配使用。
    /// </summary>
    bool Wait { get; set; }
    /// <summary>
    /// 异步形式执行动画。
    /// </summary>
    Task RunAsync();
    /// <summary>
    /// 取消动画。
    /// </summary>
    void Cancel();
}