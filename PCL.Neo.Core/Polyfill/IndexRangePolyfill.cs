using System;

namespace System
{
    /// <summary>
    /// 为.NET Standard 2.0提供C# 8.0的Index功能
    /// </summary>
    public readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;
        private readonly bool _fromEnd;

        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "值不能为负数");

            _value = value;
            _fromEnd = fromEnd;
        }

        public static Index Start => new Index(0);
        public static Index End => new Index(0, true);
        public static Index FromStart(int value) => new Index(value);
        public static Index FromEnd(int value) => new Index(value, true);

        public int Value => _value;
        public bool IsFromEnd => _fromEnd;

        public int GetOffset(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "长度不能为负数");

            if (_fromEnd)
                return length - _value;
            else
                return _value;
        }

        public override bool Equals(object obj) => obj is Index index && _value == index._value && _fromEnd == index._fromEnd;
        public bool Equals(Index other) => _value == other._value && _fromEnd == other._fromEnd;
        public override int GetHashCode() => HashCode.Combine(_value, _fromEnd);

        public static implicit operator Index(int value) => new Index(value);

        public override string ToString() => _fromEnd ? $"^{_value}" : _value.ToString();
    }

    /// <summary>
    /// 为.NET Standard 2.0提供C# 8.0的Range功能
    /// </summary>
    public readonly struct Range : IEquatable<Range>
    {
        public Index Start { get; }
        public Index End { get; }

        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        public static Range StartAt(Index start) => new Range(start, Index.End);
        public static Range EndAt(Index end) => new Range(Index.Start, end);
        public static Range All => new Range(Index.Start, Index.End);

        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start = Start.GetOffset(length);
            int end = End.GetOffset(length);

            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(length), "结束索引不能小于起始索引");

            return (start, end - start);
        }

        public override bool Equals(object obj) => obj is Range range && Start.Equals(range.Start) && End.Equals(range.End);
        public bool Equals(Range other) => Start.Equals(other.Start) && End.Equals(other.End);
        public override int GetHashCode() => HashCode.Combine(Start, End);

        public override string ToString() => $"{Start}..{End}";
    }

    /// <summary>
    /// 为.NET Standard 2.0提供HashCode功能
    /// </summary>
    internal static class HashCode
    {
        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            int h1 = value1?.GetHashCode() ?? 0;
            int h2 = value2?.GetHashCode() ?? 0;
            return ((h1 << 5) + h1) ^ h2;
        }
    }
} 