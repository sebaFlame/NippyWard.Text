using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

using NippyWard.Text;

namespace NippyWard.Text.LineFeed
{
    public interface ILineFeed
    {
        bool TryGetLineFeed
        (
            in ReadOnlySequence<byte> buffer,
            out SequencePosition lf
        );

        bool TryGetLineFeed
        (
            Utf8String @string,
            out SequencePosition lf
        )
            => this.TryGetLineFeed(@string.Buffer, out lf);

        bool TryGetLineFeed
        (
            string @string,
            out int lf
        );
    }
}
