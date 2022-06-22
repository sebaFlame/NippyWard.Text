using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace NippyWard.Text.Tests
{
    public abstract class BaseUtf8Tests
    {
        protected abstract string LowerString { get; }
        protected abstract string UpperString { get; }
        protected abstract string LowerInequalityString { get; }
        protected abstract string UpperInequalityString { get; }

        [Fact]
        public void CreationTest()
        {
            Utf8String str = new Utf8String(this.LowerString);

            Assert.NotNull(str);
        }

        [Fact]
        public void EnumeratorTest()
        {
            Utf8String str = new Utf8String(this.LowerString);

            Assert.NotNull(str);

            ReadOnlySpan<char> span = this.LowerString.AsSpan();
            Utf8CodePointEnumerator enumerator = str.GetEnumerator();

            while(enumerator.MoveNext())
            {
                Assert.Equal
                (
                    OperationStatus.Done,
                    Rune.DecodeFromUtf16(span, out Rune result, out int length)
                );

                uint c2 = enumerator.Current;
                Assert.Equal((uint)result.Value, c2);

                span = span.Slice(length);
            }
        }

        [Fact]
        public void EqualityTest()
        {
            Utf8String lStr = new Utf8String(this.LowerString);
            Utf8String rStr = new Utf8String(this.LowerString);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            Assert.Equal(lStr, rStr);
        }

        [Fact]
        public void InequalityTest()
        {
            Utf8String lStr = new Utf8String(this.LowerString);
            Utf8String rStr = new Utf8String(this.LowerInequalityString);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            Assert.NotEqual(lStr, rStr);
        }

        [Fact]
        public void IgnoreCaseEqualityTest()
        {
            Utf8String lStr = new Utf8String(this.LowerString);
            Utf8String rStr = new Utf8String(this.UpperString);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            BaseUtf8StringComparer comparer = BaseUtf8StringComparer.OrdinalIgnoreCase;
            Assert.True(comparer.Equals(lStr, rStr));
        }

        [Fact]
        public void IgnoreCaseInequalityTest()
        {
            Utf8String lStr = new Utf8String(this.LowerString);
            Utf8String rStr = new Utf8String(this.UpperInequalityString);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            BaseUtf8StringComparer comparer = BaseUtf8StringComparer.OrdinalIgnoreCase;
            Assert.False(comparer.Equals(lStr, rStr));
        }

        [Fact]
        public void HashingTest()
        {
            HashSet<Utf8String> hash = new HashSet<Utf8String>();

            Utf8String str = new Utf8String(this.LowerString);

            Assert.NotNull(str);
            Assert.True(hash.Add(str));

            Utf8String other = new Utf8String(this.LowerString);

            Assert.NotNull(other);
            Assert.False(hash.Add(other));
        }

        [Fact]
        public void HashingIgnoreCaseTest()
        {
            HashSet<Utf8String> hash =
                new HashSet<Utf8String>(BaseUtf8StringComparer.OrdinalIgnoreCase);

            Utf8String str = new Utf8String(this.LowerString);

            Assert.NotNull(str);
            Assert.True(hash.Add(str));

            Utf8String other = new Utf8String(this.UpperString);

            Assert.NotNull(other);
            Assert.False(hash.Add(other));
        }

        [Fact]
        public void MultiSegmentCreationTest()
        {
            ReadOnlySequence<byte> sequence = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );

            Utf8String str = new Utf8String(sequence);

            Assert.NotNull(str);
        }

        [Fact]
        public void MultiSegmentEnumeratorTest()
        {
            ReadOnlySequence<byte> sequence = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );

            Utf8String str = new Utf8String(sequence);

            Assert.NotNull(str);

            ReadOnlySpan<char> span = default;
            Utf8CodePointEnumerator enumerator = str.GetEnumerator();

            while(enumerator.MoveNext())
            {
                if(span.IsEmpty)
                {
                    span = this.LowerString.AsSpan();
                }

                Assert.Equal
                (
                    OperationStatus.Done,
                    Rune.DecodeFromUtf16(span, out Rune result, out int length)
                );

                uint c2 = enumerator.Current;
                Assert.Equal((uint)result.Value, c2);

                span = span.Slice(length);
            }
        }

        [Fact]
        public void MultiSegmentEqualityTest()
        {
            ReadOnlySequence<byte> lSeq = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );
            Utf8String lStr = new Utf8String(lSeq);

            ReadOnlySequence<byte> rSeq = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );
            Utf8String rStr = new Utf8String(rSeq);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            Assert.Equal(lStr, rStr);
        }

        [Fact]
        public void MultiSegmentIgnoreCaseEqualityTest()
        {
            ReadOnlySequence<byte> lSeq = CreateSequence
            (
                this.UpperString,
                this.LowerString,
                this.UpperString,
                this.LowerString
            );
            Utf8String lStr = new Utf8String(lSeq);

            ReadOnlySequence<byte> rSeq = CreateSequence
            (
                this.LowerString,
                this.UpperString,
                this.LowerString,
                this.UpperString
            );

            Utf8String rStr = new Utf8String(rSeq);

            Assert.NotNull(lStr);
            Assert.NotNull(rStr);

            BaseUtf8StringComparer comparer = BaseUtf8StringComparer.OrdinalIgnoreCase;
            Assert.True(comparer.Equals(lStr, rStr));
        }

        [Fact]
        public void MultiSegmentHashingTest()
        {
            HashSet<Utf8String> hash = new HashSet<Utf8String>();

            ReadOnlySequence<byte> s1 = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );

            Utf8String str = new Utf8String(s1);

            Assert.NotNull(str);
            Assert.True(hash.Add(str));

            ReadOnlySequence<byte> s2 = CreateSequence
            (
                this.LowerString, this.LowerString, this.LowerString, this.LowerString
            );

            Utf8String other = new Utf8String(s2);

            Assert.NotNull(other);
            Assert.False(hash.Add(other));
        }

        [Fact]
        public void HashingMultiSegmentIgnoreCaseTest()
        {
            HashSet<Utf8String> hash =
                new HashSet<Utf8String>(BaseUtf8StringComparer.OrdinalIgnoreCase);

            ReadOnlySequence<byte> lSeq = CreateSequence
            (
                this.UpperString,
                this.LowerString,
                this.UpperString,
                this.LowerString
            );
            Utf8String str = new Utf8String(lSeq);

            Assert.NotNull(str);
            Assert.True(hash.Add(str));

            ReadOnlySequence<byte> rSeq = CreateSequence
            (
                this.LowerString,
                this.UpperString,
                this.LowerString,
                this.UpperString
            );

            Utf8String other = new Utf8String(rSeq);

            Assert.NotNull(other);
            Assert.False(hash.Add(other));
        }

        internal static ReadOnlySequence<byte> CreateSequence
        (
            params string[] strings
        )
        {
            Utf8StringSequenceSegment segment, previous = null, first = null;
            foreach(string str in strings)
            {
                segment = new Utf8StringSequenceSegment(str);

                if(previous is not null)
                {
                    previous.AddNext(segment);
                }
                else
                {
                    first = segment;
                }

                previous = segment;
            }

            return new ReadOnlySequence<byte>
            (
                first,
                0,
                previous,
                previous.Memory.Length
            );
        }
    }
}