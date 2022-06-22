using System;
using System.Buffers;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace NippyWard.Text.LineFeed
{
    internal class VectorLineFeed : ILineFeed
    {
        private static Vector128<byte> _Lf128;
        private static int _V128Count;

        static VectorLineFeed()
        {
            //fill with LF (\0x0A)
            _Lf128 = Vector128.Create((byte)0x0A);

            //put these into fields
            _V128Count = Vector128<byte>.Count;
        }

        public VectorLineFeed()
        { }

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

            if(buffer.IsSingleSegment)
            {
                b = buffer.FirstSpan;
                length = b.Length;

                if (TryGetVector128LineFeed
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

                //fast path for long lengths
                //if (length >= _V128Count)
                //{
                if(TryGetVector128LineFeed
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
                //}
                //fast path for short buffer lengths
                //else
                //{
                //    LongLineFeed.TryGetLongLineFeed
                //    (
                //        in b,
                //        length,
                //        out lfPosition
                //    );
                //}

                if (lfPosition > -1)
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
        internal static unsafe bool TryGetVector128LineFeed
        (
            in ReadOnlySpan<byte> buffer,
            int length,
            out int lf
        )
        {
            int position = 0;
            int len = length;

            fixed (byte* b = buffer)
            {
                Vector128<byte> current_bytes = Vector128<byte>.Zero;
                Vector128<byte> equal = Vector128<byte>.Zero;
                Vector128<byte> shift = Vector128<byte>.Zero;
                //provide a buffer for the tail
                byte* s = stackalloc byte[16];
                int result = 0;
                
                while(len > 0)
                {
                    if (len >= _V128Count)
                    {
                        current_bytes = Sse2.LoadVector128(b + position);
                    }
                    else
                    {
                        Buffer.MemoryCopy(b + position, s, 16, len);
                        current_bytes = Sse2.LoadVector128(s);
                    }

                    //check for newlines
                    equal = Sse2.CompareEqual
                    (
                        current_bytes,
                        _Lf128
                    );

                    //get the mask of all most significant bits
                    result = Sse2.MoveMask(equal);

                    if(result > 0)
                    {
                        //count trailing zeroes to get the correct position
                        lf = position
                            + BitOperations.TrailingZeroCount((uint)result);
                        return true;
                    }

                    position += _V128Count;
                    len = length - position;
                }
            }

            lf = -1;
            return false;
        }

        public bool TryGetLineFeed
        (
            string @string,
            out int lf
        )
        {
            throw new NotImplementedException();
        }
    }
}
