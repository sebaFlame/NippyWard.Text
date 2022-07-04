using System.Runtime.Intrinsics;

namespace NippyWard.Text.Validation
{
    internal struct ProcessedUtf8Bytes
    {
        internal Vector128<sbyte> _rawbytes;
        internal Vector128<short> _high_nibbles;
        internal Vector128<sbyte> _carried_continuations;
    };
}
