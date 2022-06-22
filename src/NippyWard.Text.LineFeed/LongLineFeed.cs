using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace NippyWard.Text.LineFeed
{
    internal class LongLineFeed : ILineFeed
    {
        private const ulong _LineFeed = 0x0A0A0A0A0A0A0A0A;

        public bool TryGetLineFeed
        (
            in ReadOnlySequence<byte> buffer,
            out SequencePosition lf
        )
        {
            int length;
            ReadOnlyMemory<byte> m;
            ReadOnlySpan<byte> b;
            long position = 0;
            int lfPosition;

            if (buffer.IsSingleSegment)
            {
                b = buffer.FirstSpan;
                length = b.Length;

                if (TryGetLongLineFeed
                (
                    in b,
                    length,
                    out lfPosition
                ))
                {
                    lf = buffer.GetPosition
                    (
                        position + lfPosition
                    );

                    return true;
                }

                lf = default;
                return false;
            }

            ReadOnlySequence<byte>.Enumerator emumerator
                = buffer.GetEnumerator();

            while (emumerator.MoveNext())
            {
                m = emumerator.Current;
                b = m.Span;

                length = b.Length;
                lfPosition = -1;

                if (TryGetLongLineFeed
                (
                    in b,
                    length,
                    out lfPosition
                ))
                {
                    lf = buffer.GetPosition
                    (
                        position + lfPosition
                    );

                    return true;
                }

                position += length;
            }

            lf = default;
            return false;
        }

        //lf is -1 when not found
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool TryGetLongLineFeed
        (
            in ReadOnlySpan<byte> buffer,
            int length,
            out int lf
        )
        {
            int position = 0;
            int len = length;

            //provide a buffer for the tail
            byte* s = stackalloc byte[8];
            ulong currentBytes;
            Span<byte> span = new Span<byte>(s, 8);

            while (len > 0)
            {
                if (len >= Marshal.SizeOf<long>())
                {
                    currentBytes = MemoryMarshal.Read<ulong>
                    (
                        buffer.Slice(position)
                    );
                }
                else
                {
                    buffer.Slice(position).CopyTo(span);
                    currentBytes = MemoryMarshal.Read<ulong>
                    (
                        span
                    );
                }

                //copied from https://github.com/lemire/Code-used-on-Daniel-Lemire-s-blog/blob/c1e2d4ab72e599071d1934d2fd1def5099baf442/2017/02/14/newlines.c#L61
                if (HasZero(currentBytes ^ _LineFeed))
                {
                    for (int i = 0; i < Marshal.SizeOf<ulong>(); i++)
                    {
                        if (buffer[position + i] == 0x0A)
                        {
                            lf = position + i;
                            return true;
                        }
                    }
                }

                position += Marshal.SizeOf<long>();
                len = length - position;
            }

            lf = -1;
            return false;
        }

        // magick provided by https://graphics.stanford.edu/~seander/bithacks.html 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasZero(ulong v)
            => (((v) - 0x0101010101010101u) & ~(v) & 0x8080808080808080u) > 0;

        public bool TryGetLineFeed(string @string, out int lf)
        {
            throw new NotImplementedException();
        }
    }
}
