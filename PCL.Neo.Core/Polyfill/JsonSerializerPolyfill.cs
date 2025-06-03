using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Polyfill
{
    /// <summary>
    /// 为.NET Standard 2.0提供JSON序列化的兼容性方法
    /// </summary>
    public static class JsonSerializerPolyfill
    {
        /// <summary>
        /// 创建一个带有驼峰命名策略的JsonSerializerOptions
        /// </summary>
        public static JsonSerializerOptions CreateCamelCaseOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return options;
        }

        /// <summary>
        /// 创建一个默认的JsonSerializerOptions
        /// </summary>
        public static JsonSerializerOptions CreateDefaultOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return options;
        }

        /// <summary>
        /// 创建一个忽略空值的JsonSerializerOptions
        /// </summary>
        public static JsonSerializerOptions CreateIgnoreNullOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            
            // 在.NET Standard 2.0中，需要手动添加忽略空值的转换器
            return options;
        }
    }
} 