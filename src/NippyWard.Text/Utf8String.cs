using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace NippyWard.Text
{
    public class Utf8String : IEquatable<Utf8String>, IEnumerable<Rune>
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
            : this(Utf8Helpers.FromUtf16(str))
        { }

        public Utf8String(Utf8Span str)
            : this(str.Buffer)
        { }

        public Utf8CodePointEnumerator GetEnumerator()
            => new Utf8CodePointEnumerator(this._buffer);

        public SequenceReader<byte> CreateSequenceReader()
            => new SequenceReader<byte>(this._buffer);

        public override bool Equals(object obj)
        {
            if (obj is Utf8String str)
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

        public static int GetHashCode
        (
            Utf8String str,
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

        public static Utf8String Copy(Utf8String str)
        {
            byte[] buf = new byte[str.Length];
            str.Buffer.CopyTo(buf);

            return new Utf8String(buf);
        }

        public static Utf8String Join(byte seperator, IEnumerable<Utf8String> strings)
        {
            IEnumerator<Utf8String> enumerator = strings.GetEnumerator();
            Utf8StringSequenceSegment first = null, last = null;

            //allocate seperator array
            byte[] seperatorArr = new byte[1];
            seperatorArr[0] = seperator;

            static Utf8StringSequenceSegment AddSequence
            (
                ref Utf8StringSequenceSegment first,
                ref Utf8StringSequenceSegment last,
                Utf8String str
            )
            {
                Utf8StringSequenceSegment previous = last, segment;

                foreach (ReadOnlyMemory<byte> memory in str.Buffer)
                {
                    segment = new Utf8StringSequenceSegment(memory);

                    if (first is null)
                    {
                        first = segment;
                        previous = first;

                        continue;
                    }

                    previous = previous.AddNext(segment);
                }

                return previous;
            }

            static Utf8StringSequenceSegment AddSeperator
            (
                Utf8StringSequenceSegment last,
                byte[] seperatorArr
            )
            {
                return last.AddNext
                (
                    new Utf8StringSequenceSegment(seperatorArr)
                );
            }

            if(enumerator.MoveNext())
            {
                last = AddSequence(ref first, ref last, enumerator.Current);
            }
            else
            {
                throw new InvalidOperationException("Sequence is empty");
            }

            while (enumerator.MoveNext())
            {
                last = AddSeperator(last, seperatorArr);
                last = AddSequence(ref first, ref last, enumerator.Current);
            }

            ReadOnlySequence<byte> sequence = new ReadOnlySequence<byte>
            (
                first,
                0,
                last,
                last.Memory.Length
            );

            return new Utf8String(sequence);
        }

        public static explicit operator string(Utf8String str)
            => new string(Utf8Helpers.FromUtf8(str._buffer).Span);

        public static explicit operator Utf8String(string str)
            => new Utf8String(Utf8Helpers.FromUtf16(str));

        public static explicit operator Utf8String(Utf8Span str)
            => new Utf8String(str.Buffer);

        IEnumerator<Rune> IEnumerable<Rune>.GetEnumerator()
        {
            RuneEnumerator enumerator = new RuneEnumerator(this);
            int cnt = 0;

            while (enumerator.MoveNext())
            {
                cnt++;
            }

            Rune[] runes = new Rune[cnt];
            enumerator.Reset();
            cnt = 0;

            while (enumerator.MoveNext())
            {
                runes[cnt++] = enumerator.Current;
            }

            return ((IEnumerable<Rune>)runes).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => (this as IEnumerable<Rune>).GetEnumerator();
    }
}
