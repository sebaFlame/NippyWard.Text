using System;


using Xunit;

namespace NippyWard.Text.Tests
{
    public class Utf8CornerCaseTests
    {
        [Fact]
        public void EndsWithNullTest()
        {
            Utf8String str = new Utf8String("aa\u0000");

            Assert.NotNull(str);

            Assert.Equal(3, str.Length);
        }

        [Fact]
        public void MultiSegmentTrailingTest()
        {
            int length = 3;
            Utf8String str = new Utf8String
            (
                BaseUtf8Tests.CreateSequence
                (
                    "a",
                    "a",
                    "a"
                )
            );

            Assert.NotNull(str);

            Assert.Equal(length, str.Length);

            Utf8CodePointEnumerator enumerator = str.GetEnumerator();
            int currentLength = 0;

            while(enumerator.MoveNext())
            {
                currentLength++;
                Assert.Equal('a', enumerator.Current);
            }

            Assert.Equal(length, currentLength);
        }

        [Fact]
        public void BMPSMPEqualityOrdinalIgnoreCaseTest()
        {

            Utf8String lStr = new Utf8String
            (
                new string
                (
                    new char[]
                    {
                        '\uD801', '\uDC3B',
                        '\uD801', '\uDC5A',
                        '\uD801', '\uDC29',
                        '\uD801', '\uDD13',
                        '\uD803', '\uDCE4',
                        '\uD801', '\uDC3B',
                        '\u0165',
                        '\u1C9E',
                        '\u03B5',
                        '\uA77D',
                        '\u015F',
                        '\u0165',
                        '\u0074',
                        '\u00E8',
                        '\u0073',
                        '\u0074'
                    }
                )
            );

            Utf8String rStr = new Utf8String
            (
                new string
                (
                    new char[]
                    {
                        '\uD801', '\uDC13',
                        '\uD801', '\uDC5A',
                        '\uD801', '\uDC01',
                        '\uD801', '\uDD13',
                        '\uD803', '\uDCA4',
                        '\uD801', '\uDC13',
                        '\u0164',
                        '\u1C9E',
                        '\u0395',
                        '\uA77D',
                        '\u015E',
                        '\u0164',
                        '\u0054',
                        '\u00C8',
                        '\u0053',
                        '\u0054'
                    }
                )
            );

            Assert.True
            (
                Utf8String.Equals
                (
                    lStr,
                    rStr,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }
    }
}
