using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.IO;

namespace System.Text.Json.Nodes
{
    /// <summary>
    /// 为.NET Standard 2.0提供JsonNode的兼容性实现
    /// </summary>
    public abstract class JsonNode
    {
        protected JsonNode() { }

        public virtual JsonNode this[int index]
        {
            get => throw new InvalidOperationException("This JsonNode does not support indexing.");
            set => throw new InvalidOperationException("This JsonNode does not support indexing.");
        }

        public virtual JsonNode this[string propertyName]
        {
            get => throw new InvalidOperationException("This JsonNode does not support indexing with a property name.");
            set => throw new InvalidOperationException("This JsonNode does not support indexing with a property name.");
        }

        public static implicit operator JsonNode(bool value) => new JsonValue<bool>(value);
        public static implicit operator JsonNode(byte value) => new JsonValue<byte>(value);
        public static implicit operator JsonNode(char value) => new JsonValue<char>(value);
        public static implicit operator JsonNode(decimal value) => new JsonValue<decimal>(value);
        public static implicit operator JsonNode(double value) => new JsonValue<double>(value);
        public static implicit operator JsonNode(float value) => new JsonValue<float>(value);
        public static implicit operator JsonNode(int value) => new JsonValue<int>(value);
        public static implicit operator JsonNode(long value) => new JsonValue<long>(value);
        public static implicit operator JsonNode(sbyte value) => new JsonValue<sbyte>(value);
        public static implicit operator JsonNode(short value) => new JsonValue<short>(value);
        public static implicit operator JsonNode(uint value) => new JsonValue<uint>(value);
        public static implicit operator JsonNode(ulong value) => new JsonValue<ulong>(value);
        public static implicit operator JsonNode(ushort value) => new JsonValue<ushort>(value);
        public static implicit operator JsonNode(string value) => value == null ? null : new JsonValue<string>(value);

        public static JsonNode Parse(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                return ParseElement(doc.RootElement);
            }
        }

        private static JsonNode ParseElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    JsonObject obj = new JsonObject();
                    foreach (JsonProperty property in element.EnumerateObject())
                    {
                        obj[property.Name] = ParseElement(property.Value);
                    }
                    return obj;

                case JsonValueKind.Array:
                    JsonArray array = new JsonArray();
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        array.Add(ParseElement(item));
                    }
                    return array;

                case JsonValueKind.String:
                    return new JsonValue<string>(element.GetString());

                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                        return new JsonValue<int>(intValue);
                    if (element.TryGetInt64(out long longValue))
                        return new JsonValue<long>(longValue);
                    if (element.TryGetDouble(out double doubleValue))
                        return new JsonValue<double>(doubleValue);
                    return new JsonValue<decimal>(element.GetDecimal());

                case JsonValueKind.True:
                    return new JsonValue<bool>(true);

                case JsonValueKind.False:
                    return new JsonValue<bool>(false);

                case JsonValueKind.Null:
                    return null;

                default:
                    throw new JsonException($"Unsupported JSON value kind: {element.ValueKind}");
            }
        }

        public virtual string ToJsonString() => "{}";
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonObject的兼容性实现
    /// </summary>
    public sealed class JsonObject : JsonNode, IEnumerable<KeyValuePair<string, JsonNode>>
    {
        private readonly Dictionary<string, JsonNode> _dictionary = new Dictionary<string, JsonNode>();

        public JsonObject() { }

        public override JsonNode this[string propertyName]
        {
            get => _dictionary.TryGetValue(propertyName, out JsonNode value) ? value : null;
            set
            {
                if (value == null)
                {
                    _dictionary.Remove(propertyName);
                }
                else
                {
                    _dictionary[propertyName] = value;
                }
            }
        }

        public void Add(string propertyName, JsonNode value)
        {
            if (value != null)
            {
                _dictionary.Add(propertyName, value);
            }
        }

        public bool Remove(string propertyName) => _dictionary.Remove(propertyName);

        public bool ContainsKey(string propertyName) => _dictionary.ContainsKey(propertyName);

        public IEnumerator<KeyValuePair<string, JsonNode>> GetEnumerator() => _dictionary.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public override string ToJsonString()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(ms))
                {
                    writer.WriteStartObject();
                    
                    foreach (var kvp in _dictionary)
                    {
                        writer.WritePropertyName(kvp.Key);
                        JsonNodeWriterHelper.WriteJsonValue(writer, kvp.Value);
                    }
                    
                    writer.WriteEndObject();
                }
                
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonArray的兼容性实现
    /// </summary>
    public sealed class JsonArray : JsonNode, IEnumerable<JsonNode>
    {
        private readonly List<JsonNode> _list = new List<JsonNode>();

        public JsonArray() { }

        public override JsonNode this[int index]
        {
            get => index >= 0 && index < _list.Count ? _list[index] : null;
            set
            {
                if (value != null)
                {
                    if (index >= _list.Count)
                    {
                        _list.Add(value);
                    }
                    else
                    {
                        _list[index] = value;
                    }
                }
            }
        }

        public void Add(JsonNode value)
        {
            if (value != null)
            {
                _list.Add(value);
            }
        }

        public bool Remove(JsonNode value) => _list.Remove(value);

        public IEnumerator<JsonNode> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public override string ToJsonString()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(ms))
                {
                    writer.WriteStartArray();
                    
                    foreach (var item in _list)
                    {
                        JsonNodeWriterHelper.WriteJsonValue(writer, item);
                    }
                    
                    writer.WriteEndArray();
                }
                
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }

    /// <summary>
    /// 为.NET Standard 2.0提供JsonValue的兼容性实现
    /// </summary>
    public sealed class JsonValue<T> : JsonNode
    {
        public T Value { get; }

        public JsonValue(T value)
        {
            Value = value;
        }

        public override string ToJsonString()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(ms))
                {
                    JsonNodeWriterHelper.WriteJsonValue(writer, this);
                }
                
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }

    // 辅助方法，用于将JsonNode写入JsonWriter
    internal static class JsonNodeWriterHelper
    {
        public static void WriteJsonValue(Utf8JsonWriter writer, JsonNode node)
        {
            if (node == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (node is JsonObject obj)
            {
                writer.WriteStartObject();
                foreach (var kvp in obj)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteJsonValue(writer, kvp.Value);
                }
                writer.WriteEndObject();
            }
            else if (node is JsonArray array)
            {
                writer.WriteStartArray();
                foreach (var item in array)
                {
                    WriteJsonValue(writer, item);
                }
                writer.WriteEndArray();
            }
            else if (node is JsonValue<string> stringValue)
            {
                writer.WriteStringValue(stringValue.Value);
            }
            else if (node is JsonValue<int> intValue)
            {
                writer.WriteNumberValue(intValue.Value);
            }
            else if (node is JsonValue<long> longValue)
            {
                writer.WriteNumberValue(longValue.Value);
            }
            else if (node is JsonValue<double> doubleValue)
            {
                writer.WriteNumberValue(doubleValue.Value);
            }
            else if (node is JsonValue<decimal> decimalValue)
            {
                writer.WriteNumberValue(decimalValue.Value);
            }
            else if (node is JsonValue<bool> boolValue)
            {
                writer.WriteBooleanValue(boolValue.Value);
            }
            else
            {
                // 尝试将其他类型转换为字符串
                writer.WriteStringValue(node.ToString());
            }
        }
    }
} 