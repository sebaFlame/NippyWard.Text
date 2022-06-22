using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippyWard.Text.LineFeed
{
    internal class LegacyLineFeed : ILineFeed
    {
        private const byte _LineFeed = 0x0A;
        private const char _CharLineFeed = (char)0x000A;

        public bool TryGetLineFeed
        (
            in ReadOnlySequence<byte> buffer,
            out SequencePosition lf
        )
        {
            lf = default;
            SequencePosition? lineFeed = buffer.PositionOf<byte>(_LineFeed);

            if(lineFeed.HasValue)
            {
                lf = lineFeed.Value;
            }

            return lineFeed.HasValue;
        }

        public bool TryGetLineFeed(string @string, out int lf)
        {
            ReadOnlySpan<char> s = @string;
            lf = s.IndexOf(_CharLineFeed);
            return lf >= 0;
        }
    }
}
