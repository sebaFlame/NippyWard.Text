using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NippyWard.Text
{
    internal static class SequenceReaderExtensions
    {
        internal static bool TryPeekUInt
        (
            ref this SequenceReader<byte> reader,
            out uint value
        )
        {
            ReadOnlySpan<byte> span = reader.UnreadSpan;

            if(span.Length < sizeof(uint))
            {
                return TryPeekMultiSegment(ref reader, out value);
            }

            value = Unsafe.ReadUnaligned<uint>
            (
                ref MemoryMarshal.GetReference(span)
            );

            return true;
        }

        private static unsafe bool TryPeekMultiSegment
        (
            ref SequenceReader<byte> reader,
            out uint value
        )
        {
            uint buffer = default;
            Span<byte> tempSpan = new Span<byte>(&buffer, sizeof(uint));

            if(!reader.TryCopyTo(tempSpan))
            {
                value = default;
                return false;
            }

            value = Unsafe.ReadUnaligned<uint>
            (
                ref MemoryMarshal.GetReference(tempSpan)
            );

            return true;
        }

        //read remaining bytes into a uint
        internal static bool TryReadRemainingUInt
        (
            ref this SequenceReader<byte> reader,
            out uint value,
            out int remaining
        )
        {
            remaining = (int)reader.Remaining;
            value = 0;

            if(remaining == 0)
            {
                return false;
            }

            if(remaining == 1)
            {
                if(reader.TryRead(out byte b))
                {
                    value = (uint)b << 24;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            ReadOnlySpan<byte> span = reader.UnreadSpan;
            if(span.Length < remaining)
            {
                return TryReadRemainingMultiSegment
                (
                    ref reader,
                    remaining,
                    out value
                );
            }

            uint buf = 0;
            Span<byte> tempSpan;
            unsafe
            {
                tempSpan = new Span<byte>
                (
                    &buf,
                    remaining
                );
            }

            span.CopyTo(tempSpan);
            value = buf;
            reader.AdvanceToEnd();

            return true;
        }

        private static bool TryReadRemainingMultiSegment
        (
            ref SequenceReader<byte> reader,
            int length,
            out uint value
        )
        {
            uint buffer = default;
            Span<byte> tempSpan;

            unsafe
            {
                tempSpan = new Span<byte>
                (
                    &buffer,
                    length
                );
            }

            if(!reader.TryCopyTo(tempSpan))
            {
                value = default;
                return false;
            }

            value = buffer;
            reader.AdvanceToEnd();

            return true;
        }
    }
}

