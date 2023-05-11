using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class ParseIntTests
    {
        [Fact]
        public void ParseSingleDigitTest()
        {
            Utf8String str = new Utf8String("2");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(2, val);
        }

        [Fact]
        public void ParseMultiDigitTest()
        {
            Utf8String str = new Utf8String("12");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(12, val);
        }

        [Fact]
        public void ParseSingleDigitNegativeTest()
        {
            Utf8String str = new Utf8String("-2");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(-2, val);
        }

        [Fact]
        public void ParseMutliDigitNegativeTest()
        {
            Utf8String str = new Utf8String("-12");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(-12, val);
        }

        [Fact]
        public void ParseMaxIntTest()
        {
            Utf8String str = new Utf8String($"{int.MaxValue}");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(int.MaxValue, val);
        }

        [Fact]
        public void ParseMinIntTest()
        {
            Utf8String str = new Utf8String($"{int.MinValue}");
            Assert.True(str.TryParseToInt(out int val));
            Assert.Equal(int.MinValue, val);
        }
    }
}
