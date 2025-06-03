using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace System.Text.Json.Nodes
{
    /// <summary>
    /// 为.NET Standard 2.0提供JsonNode的扩展方法
    /// </summary>
    public static class JsonNodeExtensions
    {
        /// <summary>
        /// 将JsonNode转换为JsonObject
        /// </summary>
        public static JsonObject AsObject(this JsonNode node)
        {
            if (node is JsonObject obj)
                return obj;
            
            throw new InvalidOperationException("The JsonNode is not a JsonObject");
        }

        /// <summary>
        /// 将JsonNode转换为JsonArray
        /// </summary>
        public static JsonArray AsArray(this JsonNode node)
        {
            if (node is JsonArray array)
                return array;
            
            throw new InvalidOperationException("The JsonNode is not a JsonArray");
        }

        /// <summary>
        /// 从JsonNode获取指定类型的值
        /// </summary>
        public static T GetValue<T>(this JsonNode node)
        {
            if (node is JsonValue<T> value)
                return value.Value;
            
            if (node is JsonValue<string> stringValue && typeof(T) != typeof(string))
            {
                // 尝试将字符串转换为目标类型
                try
                {
                    return (T)Convert.ChangeType(stringValue.Value, typeof(T));
                }
                catch
                {
                    // 转换失败
                }
            }
            
            throw new InvalidOperationException($"Cannot get value of type {typeof(T).Name} from JsonNode");
        }

        /// <summary>
        /// 反序列化JsonNode为指定类型
        /// </summary>
        public static T Deserialize<T>(this JsonNode node, JsonSerializerOptions? options = null)
        {
            string json = node.ToJsonString();
            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// 将对象序列化为JsonNode
        /// </summary>
        public static JsonNode SerializeToNode<T>(T value, JsonSerializerOptions? options = null)
        {
            string json = JsonSerializer.Serialize(value, options);
            return JsonNode.Parse(json);
        }

        /// <summary>
        /// 获取JsonNode的值类型
        /// </summary>
        public static JsonValueKind GetValueKind(this JsonNode node)
        {
            if (node == null)
                return JsonValueKind.Null;
            if (node is JsonObject)
                return JsonValueKind.Object;
            if (node is JsonArray)
                return JsonValueKind.Array;
            if (node is JsonValue<string>)
                return JsonValueKind.String;
            if (node is JsonValue<int> || node is JsonValue<long> || node is JsonValue<double> || node is JsonValue<decimal>)
                return JsonValueKind.Number;
            if (node is JsonValue<bool> boolValue)
                return boolValue.Value ? JsonValueKind.True : JsonValueKind.False;

            return JsonValueKind.Undefined;
        }

        /// <summary>
        /// 尝试获取JsonObject的属性值
        /// </summary>
        public static bool TryGetPropertyValue(this JsonObject jsonObject, string propertyName, out JsonNode value)
        {
            if (jsonObject.ContainsKey(propertyName))
            {
                value = jsonObject[propertyName];
                return true;
            }
            
            value = null;
            return false;
        }

        /// <summary>
        /// 键值对解构方法
        /// </summary>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
} 