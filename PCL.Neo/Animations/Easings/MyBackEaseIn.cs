using Avalonia.Animation.Easings;
using System;

namespace PCL.Neo.Animations.Easings
{
    public class MyBackEaseIn(EasePower power = EasePower.Middle) : Easing
    {
        private readonly double p = 3 - (int)power * 0.5;

        public override double Ease(double progress)
        {
            return Math.Pow(progress, p) * Math.Cos(1.5 * Math.PI * (1 - progress));
        }
    }
}