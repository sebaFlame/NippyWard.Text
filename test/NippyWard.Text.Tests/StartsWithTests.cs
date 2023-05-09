using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class StartsWithTests
    {
        [Fact]
        public void StartsWithAsiiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            Assert.True(str.StartsWith('H'));
        }

        [Fact]
        public void StartsWithUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            Assert.True(str.StartsWith('с'));
        }

        [Fact]
        public void StartsWithFailAsiiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            Assert.False(str.StartsWith('e'));
        }

        [Fact]
        public void StartsWithFailUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            Assert.False(str.StartsWith('й'));
        }

        [Fact]
        public void StartsWithEmptyTest()
        {
            Utf8String str = Utf8String.Empty;
            Assert.False(str.StartsWith('H'));
        }
    }
}
