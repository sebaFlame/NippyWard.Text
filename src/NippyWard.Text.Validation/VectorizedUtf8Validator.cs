using System;
using System.Buffers;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

namespace NippyWard.Text.Validation
{
    //based on
    //https://github.com/lemire/fastvalidate-utf-8/blob/master/include/simdutf8check.h
    //Documentation
    //https://lemire.me/blog/2018/05/16/validating-utf-8-strings-using-as-little-as-0-7-cycles-per-byte/
    //https://arxiv.org/abs/2010.03090
    public class VectorizedUtf8Validator : IUtf8Validator
    {
        /*
         * legal utf-8 byte sequence
         * http://www.unicode.org/versions/Unicode6.0.0/ch03.pdf - page 94
         *
         *  Code Points        1st       2s       3s       4s
         * U+0000..U+007F     00..7F
         * U+0080..U+07FF     C2..DF   80..BF
         * U+0800..U+0FFF     E0       A0..BF   80..BF
         * U+1000..U+CFFF     E1..EC   80..BF   80..BF
         * U+D000..U+D7FF     ED       80..9F   80..BF
         * U+E000..U+FFFF     EE..EF   80..BF   80..BF
         * U+10000..U+3FFFF   F0       90..BF   80..BF   80..BF
         * U+40000..U+FFFFF   F1..F3   80..BF   80..BF   80..BF
         * U+100000..U+10FFFF F4       80..8F   80..BF   80..BF
         *
         */

        private ProcessedUtf8Bytes _previous;

        public VectorizedUtf8Validator()
        {
            if (!(Sse.IsSupported
                && Sse2.IsSupported
                && Sse3.IsSupported
                && Ssse3.IsSupported))
            {
                throw new NotSupportedException("The CPU needs SSE1, SSE2 & SSSE3 support for vallidation to succeed");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractHighNibbles
        (
            in Vector128<sbyte> current_bytes,
            ref ProcessedUtf8Bytes answer
        )
        {
            //and these together by, and only the higher nibbles remain
            //0000_11110_0000_0111
            answer._high_nibbles = Sse2.And
            (
                // shift every 16 bits right by 4
                // 1110_0010_0111_1000
                // becomes
                // 0000_1110_0010_0111 
                Sse2.ShiftRightLogical
                (
                    current_bytes
                        .AsInt16(),
                    4
                ),
                //create a vectory with bytes of 
                //0000_1111_0000_1111
                Vector128.Create((byte)0x0F)
                    .AsInt16()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckSmallerThan0xF4
        (
            in Vector128<sbyte> current_bytes,
            ref Vector128<sbyte> has_error
        )
        {
            //or the current errors with possible too high values
            has_error = Sse2.Or
            (
                has_error,
                //check if greater than 0 (the byte was gt F4), to set most
                //significant bit
                Sse2.CompareGreaterThan
                (
                    //unsigned SubtractSaturate, this means the minimum value is 0
                    //and the maximum value is 255
                    //0x4F is the highest possible UTF-8 single byte value
                    //anything higher is an error (4 byte character with 5 bit
                    //leading header)
                    Sse2.SubtractSaturate
                    (
                        current_bytes.AsByte(),
                        Vector128.Create((byte)0xF4)
                    )
                    .AsSByte(),
                    Vector128<sbyte>.Zero
                )  
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<sbyte> ContinuationLengths
        (
            in Vector128<short> high_nibbles
        )
        {
            return Ssse3.Shuffle
            (
               /* every high nibble (now transformed into low nibble)
                * corresponds to an index in the lookup vector
                * 1 byte (ASCCI) -> index 0-7
                * 2 byte
                *   1100_0000 corresponds to index 12 (0000_1100)
                *   1101_0000 corresponds to index 13 (0000_1101)
                * 3 byte
                *   1110_0000 corresponds to index 14 (000_1110)
                * 4 byte
                *   1111_0000 corresponds to index 15 (0000_1111)
                * continuations
                *   1000_0000 corresponds to index 8 (0000_1000)
                *   1001_0000 corresponds to index 9 (0000_1001)
                *   1010_0000 corresponds to index 10 (0000_1010)
                *   1011_0000 corresponds to index 11 (0000_1011)
                * 
                * because no most significant bits are never set (low nibbles
                * only), a lookup always occurs into the temp table with the
                * value of the lower nibble
                */
               Vector128.Create
               (
                    1, 1, 1, 1, 1, 1, 1, 1, // 0xxx (ASCII)
                    0, 0, 0, 0,             // 10xx (continuation)
                    2, 2,                   // 110x
                    3,                      // 1110
                    4                       // 1111, next should be 0 (not checked here)
               ),
               high_nibbles.AsSByte()
           );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector128<sbyte> CarryContinuations
        (
            in Vector128<sbyte> initial_lengths,
            in Vector128<sbyte> previous_carried
        )
        {
            //subtract 1 to get the correct byte length for the current position
            //in the right1 vector (with a minimum of 0)
            // <0, 0, 2, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0>
            Vector128<byte> right1 = Sse2.SubtractSaturate
            (
                //concatenate length vectors into a 2 * 16 byte vector and shift
                //to right by 16 -1. Now you get the remaining length from
                //the previous iteration
                // <0, 1, 3, 0, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 2, 0>
                Ssse3.AlignRight
                (
                    // <1, 3, 0, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 2, 0, 2>
                    initial_lengths,
                    // <0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0>
                    previous_carried,
                    16 - 1
                )
                .AsByte(),
                Vector128.Create((byte)1)
            )
            .AsByte();

            //add back the original remaning
            //now you have a count-down (up to -1) by adding back the original
            //positions
            // <1, 3, 2, 0, 1, 2, 1, 1, 2, 1, 1, 2, 1, 2, 1, 2>
            Vector128<sbyte> sum = Sse2.Add
            (
                initial_lengths,
                right1.AsSByte()
            );

            //subtract 2 to get the correct byte length for the current position
            //in the right2 vector (with a minimum of 0)
            // <0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0>
            Vector128<byte> right2 = Sse2.SubtractSaturate
            (
                //concatenate length vectors into a 2 * 16 byte vector and shift
                //to right by 16 -1. Now you get the remaining 2 lengths from
                //the previous iteration
                // <0, 0, 1, 3, 2, 0, 1, 2, 1, 1, 2, 1, 1, 2, 1, 2>
                Ssse3.AlignRight
                (
                    // <1, 3, 2, 0, 1, 2, 1, 1, 2, 1, 1, 2, 1, 2, 1, 2>
                    sum,
                    // <0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0>
                    previous_carried,
                    16 - 2
                )
                .AsByte(),
                Vector128.Create((byte)2)
            )
            .AsByte();

            //add back the original sum
            //now you have a count-down (up to -2) by adding back the first sum
            // <1, 3, 2, 1, 1, 2, 1, 1, 2, 1, 1, 2, 1, 2, 1, 2>
            return Sse2.Add
            (
                sum,
                right2.AsSByte()
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckContinuations
        (
            in Vector128<sbyte> initial_lengths,
            in Vector128<sbyte> carries,
            ref Vector128<sbyte> has_error
        )
        {
            //if both values are the same (0), then the continuation bytes are
            //incorrect
            Vector128<sbyte> overunder = Sse2.CompareEqual
            (
                //check if the computed carried continuations have filled all 0
                //length continuations
                Sse2.CompareGreaterThan
                (
                    carries,
                    initial_lengths
                ),
                //verify with all non-continuation bytes, a continuation byte
                //has a value of 0
                Sse2.CompareGreaterThan
                (
                    initial_lengths,
                    Vector128<sbyte>.Zero
                )
            );

            //or the result together with the errors
            has_error = Sse2.Or
            (
                has_error,
                overunder
            );
        }

        // when 0xED is found, next byte must be no larger than 0x9F
        // when 0xF4 is found, next byte must be no larger than 0x8F
        // next byte must be continuation, ie sign bit is set, so signed < is ok
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckFirstContinuationMax
        (
            in Vector128<sbyte> current_bytes,
            in Vector128<sbyte> off1_current_bytes,
            ref Vector128<sbyte> has_error
        )
        {
            //ceck for 3 byte header with the (left-shifted) current_bytes
            Vector128<sbyte> maskED = Sse2.CompareEqual
            (
                off1_current_bytes,
                Vector128.Create(unchecked((sbyte)0xED))
            );

            //ceck for 4 byte header with the (left-shifted) current_bytes
            Vector128<sbyte> maskF4 = Sse2.CompareEqual
            (
                off1_current_bytes,
                Vector128.Create(unchecked((sbyte)0xF4))
            );

            //check for the 3 byte header follow byte
            //as off1_current_bytes is shifted, both these align and can be AND
            //together
            Vector128<sbyte> badfollowED = Sse2.And
            (
                Sse2.CompareGreaterThan
                (
                    current_bytes,
                    Vector128.Create(unchecked((sbyte)0x9F))
                ),
                maskED
            );

            //check for the 4 byte header follow byte
            //as off1_current_bytes is shifted, both these align and can be AND
            //together
            Vector128<sbyte> badfollowF4 = Sse2.And
            (
                Sse2.CompareGreaterThan
                (
                    current_bytes,
                    Vector128.Create(unchecked((sbyte)0x8F))
                ),
                maskF4
            );

            has_error = Sse2.Or
            (
                has_error,
                Sse2.Or
                (
                    badfollowED,
                    badfollowF4
                )
            );
        }

        // map off1_hibits => error condition
        // hibits     off1    cur
        // C       => < C2 && true  
        // E       => < E1 && < A0
        // F       => < F1 && < 90
        // else      false && false
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckOverlong
        (
            in Vector128<sbyte> current_bytes,
            in Vector128<sbyte> off1_current_bytes,
            in Vector128<short> hibits,
            in Vector128<short> previous_hibits,
            ref Vector128<sbyte> has_error
        )
        {
            //first get the high nibbles of the previous sequence and
            //concatenate with the current high nibbles
            Vector128<sbyte> off1_hibits = Ssse3.AlignRight
            (
                hibits,
                previous_hibits,
                16 - 1
            )
            .AsSByte();

            Vector128<sbyte> initial_mins;
            unchecked
            {
                //every high nibble (now transformed into low nibble)
                //corresponds to an index in the lookup vector
                //first fetch the minimum values for the 1st byte
                initial_mins = Ssse3.Shuffle
                (
                    Vector128.Create
                    (
                        -128, -128, -128, -128, -128, -128, -128, -128, //ASCII
                        -128, -128, -128, -128,   // 10xx => false
                        (sbyte)0xC2, -128,        // 110x
                        (sbyte)0xE1,              // 1110
                        (sbyte)0xF1               // 1111
                    ),
                    off1_hibits
               );
            }

            //initial_mins is computed from the shifted high bits, and compared
            //with the off1_current_bytes 1st byte
            //check if the lookup values are larger than the shifted values, if
            //so: error
            Vector128<sbyte> initial_under = Sse2.CompareGreaterThan
            (
                initial_mins,
                off1_current_bytes
            );

            Vector128<sbyte> second_mins;
            unchecked
            {
                //every high nibble (now transformed into low nibble)
                //corresponds to an index in the lookup vector
                //then fetch the minimum values for the 2nd byte
                second_mins = Ssse3.Shuffle
                (
                    Vector128.Create
                    (
                        -128, -128, -128, -128, -128, -128, -128, -128,
                        -128, -128, -128, -128,    // 10xx => false
                        127, 127,                  // 110x => true
                        (sbyte)0xA0,               // 1110
                        (sbyte)0x90                // 1111
                   ),
                   off1_hibits
                );
            }

            //second_mins is computed from the shifted high bits, and compared
            //with the current_bytes 2nd byte (because it's not shifted)
            //check if the lookup values are larger than the shifted values, if
            //so: error
            Vector128<sbyte> second_under = Sse2.CompareGreaterThan
            (
                second_mins,
                current_bytes
            );

            has_error = Sse2.Or
            (
                has_error,
                //as off1_current_bytes is shifted, both these align and can be
                //AND together
                Sse2.And
                (
                    initial_under,
                    second_under
                )
            );
        }

        //check whether the current bytes are valid UTF-8
        private static ProcessedUtf8Bytes CheckUTF8Bytes
        (
            in Vector128<sbyte> current_bytes,
            in ProcessedUtf8Bytes previous,
            ref Vector128<sbyte> has_error
        )
        {
            ProcessedUtf8Bytes pb = default;

            //assign current vectorizd work set
            pb._rawbytes = current_bytes;

            //extract the high nibble from every byte
            ExtractHighNibbles(current_bytes, ref pb);

            //check if every byte is a valid UTF-8 byte
            CheckSmallerThan0xF4(current_bytes, ref has_error);

            //get the byte length of every character (rune)
            Vector128<sbyte> initial_lengths = ContinuationLengths
            (
                pb._high_nibbles
            );

            //compute the remaining bytes for a rune in each position of the
            //vector
            pb._carried_continuations = CarryContinuations
            (
                initial_lengths,
                previous._carried_continuations
            );

            //verify all continuation bytes are in place
            CheckContinuations
            (
                initial_lengths,
                pb._carried_continuations,
                ref has_error
            );

            //get the last byte from the previous sequence
            Vector128<sbyte> off1_current_bytes = Ssse3.AlignRight
             (
                pb._rawbytes,
                previous._rawbytes,
                16 - 1
            );

            //check the first continuation byte (2nd byte) of current_bytes with
            //off1_current_bytes for 2nd byte max value violations
            CheckFirstContinuationMax
            (
                current_bytes,
                off1_current_bytes,
                ref has_error
            );

            //check for overlong sequences
            CheckOverlong
            (
                current_bytes,
                off1_current_bytes,
                pb._high_nibbles,
                previous._high_nibbles,
                ref has_error
            );

            return pb;
        }

        public bool ValidateUTF8
        (
            ReadOnlySequence<byte> buffer,
            [NotNullWhen(false)] out SequencePosition? position
        )
            => this.ValidateUTF8
            (
                buffer,
                ref this._previous,
                out position
            );

        private bool ValidateUTF8
        (
            ReadOnlySequence<byte> buffer,
            ref ProcessedUtf8Bytes previous,
            [NotNullWhen(false)] out SequencePosition? position
        )
        {
            ReadOnlySpan<byte> s;
            uint? iPos;

            if (buffer.IsSingleSegment)
            {
                s = buffer.FirstSpan;

                if (!ValidataASCIIAndUTF8
                (
                    s,
                    ref previous,
                    out iPos
                ))
                {
                    position = buffer.GetPosition(iPos.Value);
                    return false;
                }

                position = null;
                return true;
            }

            ReadOnlySequence<byte>.Enumerator enumerator
                = buffer.GetEnumerator();
            ReadOnlyMemory<byte> m;
            uint length = 0;

            while (enumerator.MoveNext())
            {
                m = enumerator.Current;
                s = m.Span;

                if(s.IsEmpty)
                {
                    continue;
                }

                if(!ValidataASCIIAndUTF8
                (
                        s,
                        ref previous,
                        out iPos
                ))
                {
                    position = buffer.GetPosition(length + iPos.Value);
                    return false;
                }

                length += (uint)s.Length;
            }

            position = null;
            return true;
        }

        public bool ValidateUTF8
        (
            ReadOnlySpan<byte> buffer,
            [NotNullWhen(false)] out uint? position
        )
            => this.ValidateUTF8
            (
                buffer,
                ref this._previous,
                out position
            );

        private bool ValidateUTF8
        (
            ReadOnlySpan<byte> buffer,
            ref ProcessedUtf8Bytes previous,
            [NotNullWhen(false)] out uint? position
        )
        {
            return ValidataASCIIAndUTF8
            (
                buffer,
                ref previous,
                out position
            );
        }

        private static bool ValidateASCII
        (
            ReadOnlySpan<byte> buffer,
            [NotNullWhen(false)] out uint? position
        )
        {
            uint len = (uint)buffer.Length;
            uint i = 0;
            int error_mask;

            ref byte src = ref MemoryMarshal.GetReference<byte>(buffer);

            if (len >= 16)
            {
                for (; i <= len - 16; i += 16)
                {
                    Vector128<sbyte> current_bytes = Unsafe.ReadUnaligned<Vector128<sbyte>>
                    (
                        ref Unsafe.Add
                        (
                            ref src,
                            i
                        )
                    );

                    //only gets the mask of the most significant bit
                    //this means it's not ASCII
                    error_mask = Sse2.MoveMask(current_bytes);

                    if(error_mask > 0)
                    {
                        position = i
                            + (uint)BitOperations.TrailingZeroCount(error_mask);
                        return false;
                    }
                }
            }

            for (; i < len; i++)
            {
                //check for most significant bit
                if ((buffer[(int)i] & 0x80) > 0)
                {
                    position = i;
                    return false;
                }
            }

            position = null;
            return true;
        }

        private static bool ValidataASCIIAndUTF8
        (
            ReadOnlySpan<byte> buffer,
            ref ProcessedUtf8Bytes previous,
            [NotNullWhen(false)] out uint? position
        )
        {
            return (ValidateASCII
            (
                buffer,
                out position
            )
                    && ResetPrevious(ref previous))
                || ValidateUTF8Core
            (
                buffer,
                ref previous,
                out position
            );
        }

        private static bool ResetPrevious
        (
            ref ProcessedUtf8Bytes previous
        )
        {
            previous = default;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckError
        (
            uint pos,
            in Vector128<sbyte> has_error,
            [NotNullWhen(false)] out uint? position
        )
        {
            int error_mask = Sse2.MoveMask(has_error);

            if (error_mask > 0)
            {
                position = pos
                    + (uint)BitOperations.TrailingZeroCount(error_mask);
                return false;
            }

            position = null;
            return true;
        }

        private static bool ValidateUTF8Core
        (
            ReadOnlySpan<byte> buffer,
            ref ProcessedUtf8Bytes previous,
            [NotNullWhen(false)] out uint? position
        )
        {
            uint i = 0;
            uint len = (uint)buffer.Length;
            Vector128<sbyte> has_error;

            ref byte src = ref MemoryMarshal.GetReference<byte>(buffer);

            //if length >= 16, read into vector
            if (len >= 16)
            {
                for (; i <= len - 16; i += 16)
                {
                    has_error = Vector128<sbyte>.Zero;

                    Vector128<sbyte> current_bytes = Unsafe.ReadUnaligned<Vector128<sbyte>>
                    (
                        ref Unsafe.Add
                        (
                            ref src,
                            (int)i
                        )
                    );

                    previous = CheckUTF8Bytes
                    (
                        current_bytes,
                        in previous,
                        ref has_error
                    );

                    if(!CheckError(i, in has_error, out position))
                    {
                        return false;
                    }
                }
            }

            //last part, copy trailing into new buffer
            if (i < len)
            {
                has_error = Vector128<sbyte>.Zero;

                unsafe
                {
                    byte* s = stackalloc byte[16];

                    Span<byte> span = new Span<byte>(s, 16);
                    ref byte d = ref MemoryMarshal.GetReference<byte>(span);

                    Unsafe.CopyBlockUnaligned
                    (
                        ref d,
                        ref Unsafe.Add
                        (
                            ref src,
                            (int)i
                        ),
                        len - i
                    );

                    Vector128<sbyte> current_bytes = Unsafe.ReadUnaligned<Vector128<sbyte>>
                    (
                        ref d
                    );

                    previous = CheckUTF8Bytes
                    (
                        current_bytes,
                        in previous,
                        ref has_error
                    );
                }

                if (!CheckError(i, in has_error, out position))
                {
                    return false;
                }
            }

            position = null;
            return true;
        }
    }
}
