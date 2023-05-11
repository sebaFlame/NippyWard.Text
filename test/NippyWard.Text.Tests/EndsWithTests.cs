using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NippyWard.Text.Tests
{
    public class EndsWithTests
    {
        [Fact]
        public void EndsWithAsiiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            Assert.True(str.EndsWith('!'));
        }

        [Fact]
        public void EndsWithShortAsiiTest()
        {
            Utf8String str = new Utf8String("yo");
            Assert.True(str.EndsWith('o'));
        }

        [Fact]
        public void EndsWithSingleAsiiTest()
        {
            Utf8String str = new Utf8String("y");
            Assert.True(str.EndsWith('y'));
        }

        [Fact]
        public void EndsWithUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            Assert.True(str.EndsWith('ю'));
        }

        [Fact]
        public void EndsWitSinglehUtf8Test()
        {
            Utf8String str = new Utf8String("ю");
            Assert.True(str.EndsWith('ю'));
        }

        [Fact]
        public void EndsWithFailAsiiTest()
        {
            Utf8String str = new Utf8String("Hello there!");
            Assert.False(str.EndsWith('e'));
        }

        [Fact]
        public void EndsWithFailUtf8Test()
        {
            Utf8String str = new Utf8String("сейчас на Десятую");
            Assert.False(str.EndsWith('я'));
        }

        [Fact]
        public void EndsWithEmptyTest()
        {
            Utf8String str = Utf8String.Empty;
            Assert.False(str.EndsWith('H'));
        }
    }
}
