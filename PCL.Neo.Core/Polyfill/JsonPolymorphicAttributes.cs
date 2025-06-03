using System;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// 为.NET Standard 2.0提供JsonPolymorphicAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class JsonPolymorphicAttribute : Attribute
    {
        public JsonPolymorphicAttribute()
        {
        }

        public string TypeDiscriminatorPropertyName { get; set; } = "$type";
        public bool IgnoreUnrecognizedTypeDiscriminators { get; set; }
        public bool UnknownDerivedTypeHandling { get; set; }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonDerivedTypeAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class JsonDerivedTypeAttribute : Attribute
    {
        public Type DerivedType { get; }
        public object TypeDiscriminator { get; }

        public JsonDerivedTypeAttribute(Type derivedType)
        {
            DerivedType = derivedType;
            TypeDiscriminator = derivedType.Name;
        }

        public JsonDerivedTypeAttribute(Type derivedType, object typeDiscriminator)
        {
            DerivedType = derivedType;
            TypeDiscriminator = typeDiscriminator;
        }
    }
} 