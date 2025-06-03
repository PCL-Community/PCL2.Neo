using System;
using System.Collections.Generic;

namespace System.Collections.Generic
{
    /// <summary>
    /// 为.NET Standard 2.0提供集合的扩展方法
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 尝试将键值对添加到字典中
        /// </summary>
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                return false;
            
            dictionary.Add(key, value);
            return true;
        }
    }
} 