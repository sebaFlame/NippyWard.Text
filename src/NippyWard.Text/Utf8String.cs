using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace NippyWard.Text
{
    public class Utf8String : IEquatable<Utf8String>
    {
        public ReadOnlySequence<byte> Buffer => this._buffer;
        public int Length => (int)this._buffer.Length;
        public bool IsEmpty => this._buffer.IsEmpty;

        public static readonly Utf8String Empty;

        private readonly ReadOnlySequence<byte> _buffer;

        private static readonly Encoding _Utf8Encoding;

        [ThreadStatic]
        private static Encoder _Encoder;

        [ThreadStatic]
        private static Decoder _Decoder;

        static Utf8String()
        {
            _Utf8Encoding = new UTF8Encoding(false, true);
            Empty = new Utf8String(ReadOnlySequence<byte>.Empty);
        }

        /// <summary>
        /// <paramref name="str" /> contains a valid (!) UTF-8 string
        /// </summary>
        public Utf8String(ReadOnlySequence<byte> str)
        {
            this._buffer = str;
        }

        /// <summary>
        /// <paramref name="str" /> contains a valid (!) UTF-8 string
        /// </summary>
        public Utf8String(ReadOnlyMemory<byte> str)
            : this(new ReadOnlySequence<byte>(str))
        { }

        public Utf8String(string str)
            : this(FromUtf16(str))
        { }

        public Utf8CodePointEnumerator GetEnumerator()
            => new Utf8CodePointEnumerator(this._buffer);

        public SequenceReader<byte> CreateSequenceReader()
            => new SequenceReader<byte>(this._buffer);

        public override bool Equals(object obj)
        {
            if(obj is Utf8String str)
            {
                return Utf8String.Equals(this, str);
            }

            return false;
        }

        public Utf8String Slice(int start, int length)
            => new Utf8String(this._buffer.Slice(start, length));

        public Utf8String Slice(int start)
            => new Utf8String(this._buffer.Slice(start));

        public bool StartsWith(uint c)
        {
            Utf8CodePointEnumerator en = this.GetEnumerator();
            return en.MoveNext() && en.Current == c;
        }

#nullable enable
        public bool Equals(Utf8String? other)
            => Utf8String.Equals(this, other);

        public bool Equals(Utf8String? other, StringComparison stringComparison)
            => Utf8String.Equals(this, other, stringComparison);

        public static bool Equals(Utf8String? left, Utf8String? right)
            => BaseUtf8StringComparer.Ordinal.Equals(left, right);

        public static int Compare
        (
            Utf8String? strA,
            Utf8String? strB,
            StringComparison stringComparison
        )
        {
            switch(stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();
                case StringComparison.Ordinal:
                    return Utf8OrdinalStringComparer
                        .Ordinal
                        .Compare(strA, strB);
                case StringComparison.OrdinalIgnoreCase:
                    return Utf8OrdinalStringComparer
                        .OrdinalIgnoreCase
                        .Compare(strA, strB);
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool Equals
        (
            Utf8String? strA,
            Utf8String? strB,
            StringComparison stringComparison
        )
        {
            switch(stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();
                case StringComparison.Ordinal:
                    return Utf8OrdinalStringComparer
                        .Ordinal
                        .Equals(strA, strB);
                case StringComparison.OrdinalIgnoreCase:
                    return Utf8OrdinalStringComparer
                        .OrdinalIgnoreCase
                        .Equals(strA, strB);
                default:
                    throw new NotImplementedException();
            }
        }
#nullable disable

        public override int GetHashCode()
            => BaseUtf8StringComparer.Ordinal.GetHashCode(this);

        public override string ToString()
            => (string)this;

        public static Utf8String Copy(Utf8String str)
        {
            byte[] buf = new byte[str.Length];
            str.Buffer.CopyTo(buf);

            return new Utf8String(buf);
        }

        public static explicit operator string(Utf8String str)
            => new string(FromUtf8(str).Span);

        public static explicit operator Utf8String(string str)
            => new Utf8String(FromUtf16(str));

        public static ReadOnlyMemory<char> FromUtf8(Utf8String str)
        {
            Decoder decoder;
            if(_Decoder is null)
            {
                _Decoder = _Utf8Encoding.GetDecoder();
            }

            decoder = _Decoder;
            decoder.Reset();

            ReadOnlySequence<byte> sequence = str._buffer;

            //should always be longer if multibyte
            char[] currentString = new char[sequence.Length];
            Memory<char> stringMemory, completeMemory
                = new Memory<char>(currentString);
            int totalStringLength = 0;
            bool completed = false;
            int charsUsed = 0;

            stringMemory = completeMemory;
            foreach(ReadOnlyMemory<byte> memory in sequence)
            {
                if(memory.Length == 0)
                {
                    continue;
                }

                unsafe
                {
                    fixed(char* c = stringMemory.Span)
                    {
                        fixed(byte* b = memory.Span)
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

                if(completed)
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
            if(_Encoder is null)
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
