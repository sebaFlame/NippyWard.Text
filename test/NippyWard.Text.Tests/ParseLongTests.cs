using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class ParseLongTests
    {
        [Fact]
        public void ParseSingleDigitTest()
        {
            Utf8String str = new Utf8String("2");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(2, val);
        }

        [Fact]
        public void ParseMultiDigitTest()
        {
            Utf8String str = new Utf8String("12");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(12, val);
        }

        [Fact]
        public void ParseSingleDigitNegativeTest()
        {
            Utf8String str = new Utf8String("-2");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(-2, val);
        }

        [Fact]
        public void ParseMultiDigitNegativeTest()
        {
            Utf8String str = new Utf8String("-12");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(-12, val);
        }

        [Fact]
        public void ParseMaxIntTest()
        {
            Utf8String str = new Utf8String($"{long.MaxValue}");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(long.MaxValue, val);
        }

        [Fact]
        public void ParseMinIntTest()
        {
            Utf8String str = new Utf8String($"{long.MinValue}");
            Assert.True(str.TryParse(out long val));
            Assert.Equal(long.MinValue, val);
        }

        [Fact]
        public void ParseOverflowTest()
        {
            Utf8String str = new Utf8String($"{(ulong)long.MaxValue + 12}");
            Assert.False(str.TryParse(out long _));
        }

        [Fact]
        public void ParseNoDigitTest()
        {
            Utf8String str = new Utf8String("12 ");
            Assert.False(str.TryParse(out long _));
        }
    }
}
