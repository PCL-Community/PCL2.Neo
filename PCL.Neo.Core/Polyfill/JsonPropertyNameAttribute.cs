using System;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// 为.NET Standard 2.0提供JsonPropertyNameAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class JsonPropertyNameAttribute : Attribute
    {
        public string Name { get; }

        public JsonPropertyNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonIgnoreAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class JsonIgnoreAttribute : Attribute
    {
        public JsonIgnoreAttribute()
        {
        }
        
        public JsonIgnoreCondition Condition { get; set; }
    }
    
    /// <summary>
    /// 为.NET Standard 2.0提供JsonIgnoreCondition的兼容性实现
    /// </summary>
    public enum JsonIgnoreCondition
    {
        Never,
        Always,
        WhenWritingDefault,
        WhenWritingNull
    }
} 