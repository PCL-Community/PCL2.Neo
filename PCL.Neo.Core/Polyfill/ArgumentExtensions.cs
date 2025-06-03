using System;

namespace System
{
    /// <summary>
    /// 为.NET Standard 2.0提供ArgumentNullException的扩展方法
    /// </summary>
    public static class ArgumentExtensions
    {
        /// <summary>
        /// 如果参数为null，则抛出ArgumentNullException异常
        /// </summary>
        public static void ThrowIfNull(this ArgumentNullException exception, object argument, string paramName)
        {
            if (argument == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// 如果参数为null，则抛出ArgumentNullException异常
        /// </summary>
        public static T ThrowIfNull<T>(this ArgumentNullException exception, T argument, string paramName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(paramName);
            
            return argument;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供Math的兼容方法
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        /// <summary>
        /// 将值限制在指定范围内
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
} 