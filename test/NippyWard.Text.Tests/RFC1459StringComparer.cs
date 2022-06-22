using System;
using System.Diagnostics.CodeAnalysis;

namespace NippyWard.Text.Tests
{
    internal class RFC1459StringComparer : BaseUtf8StringComparer
    {
        private readonly RFC1459CaseFolding _caseFolding;

        public RFC1459StringComparer()
        {
            this._caseFolding = new RFC1459CaseFolding();
        }

        public override int Compare(Utf8String x, Utf8String y)
            => this._caseFolding.CompareUsingSimpleCaseFolding(x, y);

        public override bool Equals(Utf8String x, Utf8String y)
            => this._caseFolding.CompareUsingSimpleCaseFolding(x, y) == 0;

#nullable enable annotations
        public override int GetHashCode([DisallowNull] Utf8String obj)
            => this._caseFolding.GetHashCode(obj);
#nullable disable annotations

        private class RFC1459CaseFolding : BaseUtf8SimpleCaseFolding
        {
            public RFC1459CaseFolding()
                : base
            (
                new byte[]
                {
                    (byte)'{',
                    (byte)'}',
                    (byte)'|',
                    (byte)'^'
                },
                new byte[]
                {
                    (byte)'[',
                    (byte)']',
                    (byte)'\\',
                    (byte)'~'
                }
            )
            { }
        }
    }
}
