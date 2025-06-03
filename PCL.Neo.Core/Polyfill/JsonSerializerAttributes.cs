using System;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 指定应忽略属性值的条件
    /// </summary>
    public enum JsonIgnoreConditionPolyfill
    {
        /// <summary>
        /// 指示不应忽略属性的值
        /// </summary>
        Never = 0,

        /// <summary>
        /// 指示在写入时，应忽略属性的默认值
        /// </summary>
        WhenWritingDefault = 1,

        /// <summary>
        /// 指示在写入时，应忽略属性的空值
        /// </summary>
        WhenWritingNull = 2,

        /// <summary>
        /// 指示在写入时，应忽略默认值和null值
        /// </summary>
        Always = 3
    }

    /// <summary>
    /// 指定属性或字段应被忽略的条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class JsonIgnoreAttributePolyfill : Attribute
    {
        /// <summary>
        /// 获取或设置一个值，该值指示属性值应被忽略的条件
        /// </summary>
        public JsonIgnoreConditionPolyfill Condition { get; set; } = JsonIgnoreConditionPolyfill.Never;

        /// <summary>
        /// 初始化 <see cref="JsonIgnoreAttributePolyfill"/> 的新实例
        /// </summary>
        public JsonIgnoreAttributePolyfill()
        {
        }

        /// <summary>
        /// 初始化 <see cref="JsonIgnoreAttributePolyfill"/> 的新实例，并设置 <see cref="Condition"/> 属性
        /// </summary>
        /// <param name="condition">指定属性值应被忽略的条件</param>
        public JsonIgnoreAttributePolyfill(JsonIgnoreConditionPolyfill condition)
        {
            Condition = condition;
        }
    }
} 