using System;

namespace System.Runtime.CompilerServices
{
    /* 注意：下面这些类型已由NuGet包提供，所以这里注释掉它们以避免重复定义 */
    /*
    /// <summary>
    /// 用于支持C# 9的init访问器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class IsExternalInitAttribute : Attribute
    {
    }
    */

    /*
    /// <summary>
    /// 用于支持C# 10的record struct
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        public string FeatureName { get; }
        public bool IsOptional { get; init; }
    }
    */

    /// <summary>
    /// 用于支持C# 10的record struct
    /// </summary>
    internal static class RequiredMemberAttributeNames
    {
        public const string RefStructs = "RefStructs";
        public const string RequiredMembers = "RequiredMembers";
    }

    /*
    /// <summary>
    /// 用于支持C# 11的必需成员属性支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class RequiredMemberAttribute : Attribute
    {
    }
    */

    /// <summary>
    /// 用于支持C# 11的SetsRequiredMembers属性支持
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
} 