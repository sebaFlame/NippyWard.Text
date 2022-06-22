using System.Diagnostics.CodeAnalysis;

namespace NippyWard.Text
{
    public class Utf8OrdinalIgnoreCaseStringComparer : BaseUtf8StringComparer
    {
        public static Utf8OrdinalIgnoreCaseStringComparer Instance;

        static Utf8OrdinalIgnoreCaseStringComparer()
        {
            Instance = new();
        }

        public override int Compare(Utf8String x, Utf8String y)
            => Utf8SimpleCaseFolding.Instance.CompareUsingSimpleCaseFolding(x, y);

        public override bool Equals(Utf8String x, Utf8String y)
            => Utf8SimpleCaseFolding.Instance.CompareUsingSimpleCaseFolding(x, y) == 0;

        public override int GetHashCode([DisallowNull] Utf8String obj)
            => Utf8SimpleCaseFolding.Instance.GetHashCode(obj);
    }
}
