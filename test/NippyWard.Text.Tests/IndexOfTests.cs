using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class IndexOfTests
    {
        [Fact]
        public void IndexOfAsiiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            long idx = str.IndexOf(0x20);
            Assert.Equal(5, idx);
        }

        [Fact]
        public void IndexOfUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");

            //this counts the bytes, not the characters, this way index is
            //compatible with slice
            long idx = str.IndexOf(new Rune('Д'));
            Assert.Equal(18, idx);
        }

        [Fact]
        public void IndexOfFirstCharAsciiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            long idx = str.IndexOf((byte)'H');
            Assert.Equal(0, idx);
        }

        [Fact]
        public void IndexOfFirstCharUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            long idx = str.IndexOf(new Rune('с'));
            Assert.Equal(0, idx);
        }

        [Fact]
        public void IndexOfNotFoundAsciiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            long idx = str.IndexOf((byte)'.');
            Assert.Equal(-1, idx);
        }

        [Fact]
        public void IndexOfNotFoundUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            long idx = str.IndexOf(new Rune('К'));
            Assert.Equal(-1, idx);
        }

        [Fact]
        public void IndexOfEmptyStringAsciiTest()
        {
            Utf8String str = Utf8String.Empty;
            long idx = str.IndexOf(0x20);
            Assert.Equal(-1, idx);
        }

        [Fact]
        public void IndexOfEmptyStringUtf8Test()
        {
            Utf8String str = Utf8String.Empty;
            long idx = str.IndexOf(new Rune('К'));
            Assert.Equal(-1, idx);
        }

        [Fact]
        public void IndexOfMultiSegmentAsciiTest()
        {
            ReadOnlyMemory<byte> p1 = Utf8String.FromUtf16("Hello");
            ReadOnlyMemory<byte> p2 = Utf8String.FromUtf16(" there!");

            Utf8StringSequenceSegment s1 = new Utf8StringSequenceSegment(p1);
            Utf8StringSequenceSegment s2 = new Utf8StringSequenceSegment(p2);
            s1.AddNext(s2);

            ReadOnlySequence<byte> seq = s1.CreateReadOnlySequence(s2);

            Utf8String str = new Utf8String(seq);

            long idx = str.IndexOf(0x20);
            Assert.Equal(5, idx);
        }

        [Fact]
        public void IndexOfMultiSegmentUtf8Test()
        {
            ReadOnlyMemory<byte> p1 = Utf8String.FromUtf16("сейчас на");
            ReadOnlyMemory<byte> multi = Utf8String.FromUtf16(" Десятую");
            ReadOnlyMemory<byte> p2 = multi.Slice(0, 2); //slice in the middle of searched char
            ReadOnlyMemory<byte> p3 = multi.Slice(2);


            Utf8StringSequenceSegment s1 = new Utf8StringSequenceSegment(p1);
            Utf8StringSequenceSegment s2 = new Utf8StringSequenceSegment(p2);
            Utf8StringSequenceSegment s3 = new Utf8StringSequenceSegment(p3);

            s1
                .AddNext(s2)
                .AddNext(s3);

            ReadOnlySequence<byte> seq = s1.CreateReadOnlySequence(s3);

            Utf8String str = new Utf8String(seq);

            long idx = str.IndexOf(new Rune('Д'));
            Assert.Equal(18, idx);
        }
    }
}
