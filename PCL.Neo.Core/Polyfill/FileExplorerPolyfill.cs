using System;
using System.IO;

namespace System.IO
{
    /// <summary>
    /// 为.NET Standard 2.0提供EnumerationOptions类
    /// </summary>
    public class EnumerationOptions
    {
        /// <summary>
        /// 获取或设置一个值，该值指示是否应遵循符号链接。
        /// </summary>
        public bool FollowSymbolicLinks { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示是否应忽略不可访问的目录。
        /// </summary>
        public bool IgnoreInaccessible { get; set; } = true;

        /// <summary>
        /// 获取或设置一个值，该值指示是否应匹配所有文件系统条目
        /// </summary>
        public bool MatchAll { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示是否应区分大小写。
        /// </summary>
        public bool MatchCasing { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示在枚举期间应使用的最大目录深度。
        /// </summary>
        public int MaxRecursionDepth { get; set; } = int.MaxValue;

        /// <summary>
        /// 获取或设置一个值，该值指示目录中的条目是否应按字母顺序返回。
        /// </summary>
        public bool ReturnSpecialDirectories { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值，该值指示是否应按特定顺序返回目录中的条目。
        /// </summary>
        public bool RecurseSubdirectories { get; set; } = false;
    }
} 