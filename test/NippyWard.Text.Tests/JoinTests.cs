using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class JoinTests
    {
        [Fact]
        public void JoinTwoStringsWithCommaTest()
        {
            Utf8String[] strings = new Utf8String[]
            {
                new Utf8String("first"),
                new Utf8String("second")
            };

            Utf8String joined = Utf8String.Join((byte)',', strings);

            Assert.Equal
            (
                string.Join(',', "first", "second"),
                (string)joined
            );
        }

        [Fact]
        public void JoinMultipleStringsWithCommaTest()
        {
            Utf8String[] strings = new Utf8String[]
            {
                new Utf8String("first"),
                new Utf8String("second"),
                new Utf8String("third")
            };

            Utf8String joined = Utf8String.Join((byte)',', strings);

            Assert.Equal
            (
                string.Join(',', "first", "second", "third"),
                (string)joined
            );
        }

        [Fact]
        public void JoinEmptyWithCommaTest()
        {
            Utf8String[] strings = Array.Empty<Utf8String>();

            Assert.Throws<InvalidOperationException>
            (
                () => Utf8String.Join((byte)',', strings)
            );
        }

        [Fact]
        public void JoinEmptyStringsWithCommaTest()
        {
            Utf8String[] strings = new Utf8String[]
            {
                new Utf8String(""),
                new Utf8String("")
            };

            Utf8String joined = Utf8String.Join((byte)',', strings);

            Assert.Equal
            (
                string.Join(',', "", ""),
                (string)joined
            );
        }

        [Fact]
        public void JoinSingleStringWithCommaTest()
        {
            Utf8String[] strings = new Utf8String[]
            {
                new Utf8String("first")
            };

            Utf8String joined = Utf8String.Join((byte)',', strings);

            Assert.Equal
            (
                string.Join(',', "first"),
                (string)joined
            );
        }
    }
}
