using System;

namespace System.Text.RegularExpressions
{
    /// <summary>
    /// 为.NET Standard 2.0提供GeneratedRegexAttribute的兼容性实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class GeneratedRegexAttribute : Attribute
    {
        public string Pattern { get; }
        public RegexOptions Options { get; }
        public int MatchTimeoutMilliseconds { get; }
        
        public GeneratedRegexAttribute(string pattern)
        {
            Pattern = pattern;
            Options = RegexOptions.None;
            MatchTimeoutMilliseconds = -1;
        }
        
        public GeneratedRegexAttribute(string pattern, RegexOptions options)
        {
            Pattern = pattern;
            Options = options;
            MatchTimeoutMilliseconds = -1;
        }
        
        public GeneratedRegexAttribute(string pattern, RegexOptions options, int matchTimeoutMilliseconds)
        {
            Pattern = pattern;
            Options = options;
            MatchTimeoutMilliseconds = matchTimeoutMilliseconds;
        }
    }
} 