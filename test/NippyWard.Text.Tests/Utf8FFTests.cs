using System;

using Xunit;

namespace NippyWard.Text.Tests
{
    public class Utf8FFTests : BaseUtf8Tests
    {
        protected override string LowerString => new string
        (
            new char[]
            {
                '\u0074',
                '\u00E8',
                '\u0073',
                '\u0074'
            }
        );

        protected override string UpperString => new string
        (
            new char[]
            {
                '\u0054',
                '\u00C8',
                '\u0053',
                '\u0054'
            }
        );

        protected override string LowerInequalityString => new string
        (
            new char[]
            {
                '\u0074',
                '\u00E8',
                '\u0073',
                '\u0064'
            }
        );

        protected override string UpperInequalityString => new string
        (
            new char[]
            {
                '\u0054',
                '\u00C8',
                '\u0053',
                '\u0044'
            }
        );

        [Fact]
        public void OrdinalComparerTest()
        {
            Utf8String s1 = new Utf8String("Hello");
            Utf8String s1a = new Utf8String("Hello");
            Utf8String s2 = new Utf8String("HELLO");
            Utf8String s3 = new Utf8String("There");

            Utf8String n1 = null, n2 = null;

            //2 nulls are same
            Assert.Equal
            (
                0,
                Utf8String.Compare(n1, n2, StringComparison.Ordinal)
            );

            //1 null, not same
            Assert.NotEqual
            (
                0,
                Utf8String.Compare(s1, n2, StringComparison.Ordinal)
            );

            //same reference
            Assert.Equal
            (
                0,
                Utf8String.Compare(s1, s1, StringComparison.Ordinal)
            );

            //same value
            Assert.Equal
            (
                0,
                Utf8String.Compare(s1, s1a, StringComparison.Ordinal)
            );

            //s2 should come first
            Assert.True
            (
                Utf8String.Compare(s1, s2, StringComparison.Ordinal) > 0
            );

            //s2 should come first
            Assert.True
            (
                Utf8String.Compare(s2, s1, StringComparison.Ordinal) < 0
            );

            //s1 should come first
            Assert.True
            (
                Utf8String.Compare(s1, s3, StringComparison.Ordinal) < 0
            );

            //s1 should come first
            Assert.True
            (
                Utf8String.Compare(s3, s1, StringComparison.Ordinal) > 0
            );
        }

        [Fact]
        public void OrdinalIgnoreCaseComparerTest()
        {
            Utf8String s1 = new Utf8String("Hello");
            Utf8String s1a = new Utf8String("Hello");
            Utf8String s2 = new Utf8String("HELLO");
            Utf8String s3 = new Utf8String("There");

            Utf8String n1 = null, n2 = null;

            //2 nulls are same
            Assert.Equal
            (
                0,
                Utf8String.Compare(n1, n2, StringComparison.OrdinalIgnoreCase)
            );

            //1 null, not same
            Assert.NotEqual
            (
                0,
                Utf8String.Compare(s1, n2, StringComparison.OrdinalIgnoreCase)
            );

            //same reference
            Assert.Equal
            (
                0,
                Utf8String.Compare(s1, s1, StringComparison.OrdinalIgnoreCase)
            );

            //same value
            Assert.Equal
            (
                0,
                Utf8String.Compare(s1, s1a, StringComparison.OrdinalIgnoreCase)
            );

            //should be same
            Assert.Equal
            (
                0,
                Utf8String.Compare(s1, s2, StringComparison.OrdinalIgnoreCase)
            );

            Assert.Equal
            (
                0,
                Utf8String.Compare(s2, s1, StringComparison.OrdinalIgnoreCase)
            );

            //s1 should come first
            Assert.True
            (
                Utf8String.Compare
                (
                    s1,
                    s3,
                    StringComparison.OrdinalIgnoreCase
                ) < 0
            );

            //s1 should come first
            Assert.True
            (
                Utf8String.Compare
                (
                    s3,
                    s1,
                    StringComparison.OrdinalIgnoreCase
                ) > 0
            );
        }

    }
}