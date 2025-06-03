using System;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// 为.NET Standard 2.0提供JsonStringEnumConverter的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class JsonStringEnumConverterAttribute : JsonConverterAttribute
    {
        public JsonStringEnumConverterAttribute() : base(typeof(JsonStringEnumConverter))
        {
        }

        public JsonStringEnumConverterAttribute(Type converterType) : base(converterType)
        {
        }

        public string DeserializationFailureFallbackValue { get; set; }
        public bool AllowIntegerValues { get; set; } = true;
        public JsonNamingPolicy NamingPolicy { get; set; }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonStringEnumConverter的兼容性实现
    /// </summary>
    public class JsonStringEnumConverter : JsonConverter
    {
        public JsonStringEnumConverter()
        {
        }

        public JsonStringEnumConverter(JsonNamingPolicy namingPolicy = null, bool allowIntegerValues = true)
        {
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonStringEnumMemberNameAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class JsonStringEnumMemberNameAttribute : Attribute
    {
        public string Name { get; }

        public JsonStringEnumMemberNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonConverterAttribute的基本兼容性实现
    /// </summary>
    public abstract class JsonConverter
    {
        public virtual Type TypeToConvert { get; }

        protected JsonConverter()
        {
        }

        protected JsonConverter(Type typeToConvert)
        {
            TypeToConvert = typeToConvert;
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonNamingPolicy的兼容性实现
    /// </summary>
    public abstract class JsonNamingPolicy
    {
        public static JsonNamingPolicy CamelCase { get; } = new CamelCaseNamingPolicy();

        public virtual string ConvertName(string name)
        {
            return name;
        }

        private class CamelCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
                {
                    return name;
                }

                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonConverterAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false)]
    public class JsonConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public JsonConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public virtual JsonConverter CreateConverter(Type typeToConvert)
        {
            return null;
        }
    }
} 