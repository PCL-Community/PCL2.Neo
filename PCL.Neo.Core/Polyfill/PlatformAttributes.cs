using System;

namespace System.Runtime.Versioning
{
    /// <summary>
    /// 为.NET Standard 2.0提供SupportedOSPlatformAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class SupportedOSPlatformAttribute : Attribute
    {
        public string PlatformName { get; }

        public SupportedOSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供UnsupportedOSPlatformAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class UnsupportedOSPlatformAttribute : Attribute
    {
        public string PlatformName { get; }

        public UnsupportedOSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供ObsoletedOSPlatformAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ObsoletedOSPlatformAttribute : Attribute
    {
        public string PlatformName { get; }
        public string Message { get; }

        public ObsoletedOSPlatformAttribute(string platformName)
        {
            PlatformName = platformName;
            Message = null;
        }

        public ObsoletedOSPlatformAttribute(string platformName, string message)
        {
            PlatformName = platformName;
            Message = message;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供SupportedOSPlatformGuardAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class SupportedOSPlatformGuardAttribute : Attribute
    {
        public string PlatformName { get; }

        public SupportedOSPlatformGuardAttribute(string platformName)
        {
            PlatformName = platformName;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供UnsupportedOSPlatformGuardAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class UnsupportedOSPlatformGuardAttribute : Attribute
    {
        public string PlatformName { get; }

        public UnsupportedOSPlatformGuardAttribute(string platformName)
        {
            PlatformName = platformName;
        }
    }
} 