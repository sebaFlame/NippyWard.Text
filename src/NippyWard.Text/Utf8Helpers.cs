using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippyWard.Text
{
    public static class Utf8Helpers
    {
        private static readonly Encoding _Utf8Encoding;

        [ThreadStatic]
        private static Encoder _Encoder;

        [ThreadStatic]
        private static Decoder _Decoder;

        static Utf8Helpers()
        {
            _Utf8Encoding = new UTF8Encoding(false, true);
        }

        internal static bool StartsWith(Utf8CodePointEnumerator en, uint c)
        {
            return en.MoveNext() && en.Current == c;
        }

        internal static bool EndsWith(Utf8CodePointEnumerator en, uint c)
        {
            return en.Last == c;
        }

        internal static long IndexOf
        (
            Utf8CodePointEnumerator en,
            uint c
        )
        {
            while (en.MoveNext())
            {
                if (c == en.Current)
                {
                    return en.Index;
                }
            }

            return -1;
        }

        internal static long IndexOf(Utf8CodePointEnumerator en, Rune r)
            => IndexOf(en, (uint)r.Value);

        internal static long IndexOf(SequenceReader<byte> reader, byte b)
        {
            if (!reader.TryAdvanceTo(b, false))
            {
                return -1;
            }

            return reader.Consumed;
        }

        internal static bool TryParse
        (
            SequenceReader<byte> reader,
            out int val
        )
        {
            if(reader.Length == 0)
            {
                val = 0;
                return false;
            }

            int sign = 1;
            int answer = 0;

            if (reader.TryPeek(out byte neg)
                && neg == 0x2D)
            {
                sign = -1;
                reader.Advance(1);
            }

            while (reader.TryRead(out byte v))
            {
                if (v >= 0x30
                    && v <= 0x39)
                {
                    answer = answer * 10 + (v - 0x30);

                    //overflow
                    if ((uint)answer > (uint)int.MaxValue + (-1 * sign + 1) / 2)
                    {
                        val = default;
                        return false;
                    }
                }
                else
                {
                    val = default;
                    return false;
                }
            }

            val = answer * sign;
            return true;
        }

        internal static bool TryParse
        (
            SequenceReader<byte> reader,
            out long val
        )
        {
            if (reader.Length == 0)
            {
                val = 0;
                return false;
            }

            int sign = 1;
            long answer = 0;

            if (reader.TryPeek(out byte neg)
                && neg == 0x2D)
            {
                sign = -1;
                reader.Advance(1);
            }

            while (reader.TryRead(out byte v))
            {
                if (v >= 0x30
                    && v <= 0x39)
                {
                    answer = answer * 10 + (v - 0x30);

                    //overflow
                    if ((ulong)answer > (ulong)(long.MaxValue + (-1 * sign + 1) / 2))
                    {
                        val = default;
                        return false;
                    }
                }
                else
                {
                    val = default;
                    return false;
                }
            }

            val = answer * sign;
            return true;
        }

        public static ReadOnlyMemory<char> FromUtf8
        (
            in ReadOnlySequence<byte> sequence
        )
        {
            Decoder decoder;
            if (_Decoder is null)
            {
                _Decoder = _Utf8Encoding.GetDecoder();
            }

            decoder = _Decoder;
            decoder.Reset();

            //should always be longer if multibyte
            char[] currentString = new char[sequence.Length];
            Memory<char> stringMemory, completeMemory
                = new Memory<char>(currentString);
            int totalStringLength = 0;
            bool completed = false;
            int charsUsed = 0;

            stringMemory = completeMemory;
            foreach (ReadOnlyMemory<byte> memory in sequence)
            {
                if (memory.Length == 0)
                {
                    continue;
                }

                unsafe
                {
                    fixed (char* c = stringMemory.Span)
                    {
                        fixed (byte* b = memory.Span)
                        {
                            decoder.Convert
                            (
                                b,
                                memory.Span.Length,
                                c,
                                stringMemory.Length,
                                false,
                                out int _,
                                out charsUsed,
                                out completed
                            );
                        }
                    }
                }

                if (completed)
                {
                    totalStringLength += charsUsed;
                    stringMemory = stringMemory.Slice(charsUsed);
                }
                else
                {
                    break;
                }
            }

            return completeMemory.Slice(0, totalStringLength);
        }

        public static ReadOnlyMemory<byte> FromUtf16(string str)
        {
            if (_Encoder is null)
            {
                _Encoder = _Utf8Encoding.GetEncoder();
            }

            _Encoder.Reset();

            ReadOnlySpan<char> chars = str.AsSpan();

            byte[] buf = new byte[_Encoder.GetByteCount(chars, true)];

            Memory<byte> utf8 = new Memory<byte>(buf);

            _Encoder.GetBytes(chars, utf8.Span, true);

            return utf8;
        }
    }
}
