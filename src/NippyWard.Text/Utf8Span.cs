using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace NippyWard.Text
{
    //a stack-only variant of Utf8String
    public readonly ref struct Utf8Span
    {
        public ReadOnlySequence<byte> Buffer => this._buffer;
        public int Length => (int)this._buffer.Length;
        public bool IsEmpty => this._buffer.IsEmpty;

        public static Utf8Span Empty => default;

        private readonly ReadOnlySequence<byte> _buffer;

        /// <summary>
        /// <paramref name="str" /> contains a valid (!) UTF-8 string
        /// </summary>
        public Utf8Span(ReadOnlySequence<byte> str)
        {
            this._buffer = str;
        }

        /// <summary>
        /// <paramref name="str" /> contains a valid (!) UTF-8 string
        /// </summary>
        public Utf8Span(ReadOnlyMemory<byte> str)
            : this(new ReadOnlySequence<byte>(str))
        { }

        public Utf8Span(string str)
            : this(Utf8Helpers.FromUtf16(str))
        { }

        public Utf8Span(Utf8String str)
            : this(str.Buffer)
        { }

        public Utf8CodePointEnumerator GetEnumerator()
            => new Utf8CodePointEnumerator(this._buffer);

        public SequenceReader<byte> CreateSequenceReader()
            => new SequenceReader<byte>(this._buffer);

        public Utf8Span Slice(int start, int length)
            => new Utf8Span(this._buffer.Slice(start, length));

        public Utf8Span Slice(int start)
            => new Utf8Span(this._buffer.Slice(start));

        public bool StartsWith(uint c)
            => Utf8Helpers.StartsWith(this.GetEnumerator(), c);

        public bool EndsWith(uint c)
            => Utf8Helpers.EndsWith(this.GetEnumerator(), c);

        /// <summary>
        /// Find the byte (!) index of any unicode encoded codepoint
        /// </summary>
        /// <param name="c">The codepoint to find</param>
        /// <returns>The index or -1 if not found</returns>
        public long IndexOf(uint c)
            => Utf8Helpers.IndexOf(this.GetEnumerator(), c);

        /// <summary>
        /// Find the byte (!) index of any rune
        /// </summary>
        /// <param name="c">The rune to find</param>
        /// <returns>The index or -1 if not found</returns>
        public long IndexOf(Rune r)
             => Utf8Helpers.IndexOf(this.GetEnumerator(), r);

        /// <summary>
        /// Find any ASCII encoded codepoint.
        /// </summary>
        /// <param name="b">The ascii char to find</param>
        /// <returns>The index or -1 if not found</returns>
        public long IndexOf(byte b)
            => Utf8Helpers.IndexOf(this.CreateSequenceReader(), b);

        public bool TryParseToInt(out int val)
            => Utf8Helpers.TryParseToInt(this.CreateSequenceReader(), out val);

        public override int GetHashCode()
            => BaseUtf8StringComparer.Ordinal.GetHashCode(this);

        public override string ToString()
            => (string)this;

        public static int GetHashCode
        (
            Utf8Span str,
            StringComparison stringComparison
        )
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();
                case StringComparison.Ordinal:
                    return Utf8OrdinalStringComparer
                        .Ordinal
                        .GetHashCode(str);
                case StringComparison.OrdinalIgnoreCase:
                    return Utf8OrdinalStringComparer
                        .OrdinalIgnoreCase
                        .GetHashCode(str);
                default:
                    throw new NotImplementedException();
            }
        }

        public static int Compare
        (
            Utf8Span l,
            Utf8Span r,
            StringComparison stringComparison
        )
        {
            switch (stringComparison)
            {
                case StringComparison.CurrentCulture:
                case StringComparison.CurrentCultureIgnoreCase:
                case StringComparison.InvariantCulture:
                case StringComparison.InvariantCultureIgnoreCase:
                    throw new NotImplementedException();
                case StringComparison.Ordinal:
                    return Utf8OrdinalStringComparer
                        .Ordinal
                        .Compare(l, r);
                case StringComparison.OrdinalIgnoreCase:
                    return Utf8OrdinalStringComparer
                        .OrdinalIgnoreCase
                        .Compare(l, r);
                default:
                    throw new NotImplementedException();
            }
        }

        public static Utf8Span Copy(Utf8Span str)
        {
            byte[] buf = new byte[str.Length];
            str.Buffer.CopyTo(buf);

            return new Utf8Span(buf);
        }

        public static explicit operator string(Utf8Span str)
            => new string(Utf8Helpers.FromUtf8(str._buffer).Span);

        public static explicit operator Utf8Span(string str)
            => new Utf8Span(Utf8Helpers.FromUtf16(str));

        public static explicit operator Utf8Span(Utf8String str)
            => new Utf8Span(str.Buffer);
    }
}
