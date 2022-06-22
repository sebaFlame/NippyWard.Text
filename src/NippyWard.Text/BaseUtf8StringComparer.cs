using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NippyWard.Text
{
    public abstract class BaseUtf8StringComparer
        : IComparer<Utf8String>, IEqualityComparer<Utf8String>
    {
        public static BaseUtf8StringComparer Ordinal
            => Utf8OrdinalStringComparer.Instance;

        public static BaseUtf8StringComparer OrdinalIgnoreCase
            => Utf8OrdinalIgnoreCaseStringComparer.Instance;

        public abstract int Compare(Utf8String x, Utf8String y);
        public abstract bool Equals(Utf8String x, Utf8String y);
        public abstract int GetHashCode([DisallowNull] Utf8String obj);
    }
}