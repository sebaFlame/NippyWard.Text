using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace NippyWard.Text.LineFeed
{
    public class LineFeed : ILineFeed
    {
        private readonly ILineFeed _instance;

        public LineFeed()
        {
#if ENABLEOWNOVERRIDE
            if (!(Sse.IsSupported
                && Sse2.IsSupported))
            {
                this._instance = new LongLineFeed();
            }
            else
            {
                this._instance = new VectorLineFeed();
            }
#else
            //after (!) I wrote VectorLineFeed I discovered the dotnet already
            //contains a vectorized implementation
            //https://github.com/dotnet/runtime/blob/fdc3b51da075066a5da489be2c169be85dbc117f/src/libraries/System.Private.CoreLib/src/System/SpanHelpers.Byte.cs#L437
            this._instance = new LegacyLineFeed();
#endif
        }

        public bool TryGetLineFeed(in ReadOnlySequence<byte> buffer, out SequencePosition lf)
            => this._instance.TryGetLineFeed(in buffer, out lf);

        public bool TryGetLineFeed(string @string, out int lf)
            => this._instance.TryGetLineFeed(@string, out lf);
    }
}
