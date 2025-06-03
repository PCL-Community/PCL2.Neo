using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 为.NET Standard 2.0提供C# 9.0的init-only属性支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ModuleInitializerAttribute : Attribute
    {
    }

    /// <summary>
    /// 为.NET Standard 2.0提供C# 9.0的init-only属性支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供C# 10的字符串内插支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerAttribute : Attribute
    {
    }

    /// <summary>
    /// 为.NET Standard 2.0提供C# 10的字符串内插支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class InterpolatedStringHandlerArgumentAttribute : Attribute
    {
        public InterpolatedStringHandlerArgumentAttribute(string[] argumentNames)
        {
            ArgumentNames = argumentNames;
        }

        public string[] ArgumentNames { get; }
    }
} 