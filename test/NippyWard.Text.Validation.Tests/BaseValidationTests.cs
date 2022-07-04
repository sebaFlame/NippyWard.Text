using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NippyWard.Text.Validation.Tests
{
    public abstract class BaseValidationTests
    {
        public static IEnumerable<object[]> _GoodSequences
            = new object[][]
            {
                new object[]
                {
                    new byte[] { (byte)'a' },
                },
                new object[]
                {
                    CreateGoodSequence(),
                },
                new object[]
                {
                    new byte[] { 0xc3, 0xb1 },
                },
                new object[]
                {
                    new byte[] { 0xe2, 0x82, 0xa1 },
                },
                new object[]
                {
                    new byte[] { 0xf0, 0x90, 0x8c, 0xbc }
                }
            };

        public static IEnumerable<object[]> _BadSequences
            = new object[][]
            {
                new object[]
                {
                    CreateBadSequence()
                },
                new object[]
                {
                    new byte[] { 0xc3, 0x28 }
                },
                new object[]
                {
                    new byte[] { 0xa0, 0xa1 }
                },
                new object[]
                {
                    new byte[] { 0xe2, 0x28, 0xa1 }
                },
                new object[]
                {
                    new byte[] { 0xe2, 0x82, 0x28 }
                },
                new object[]
                {
                    new byte[] { 0xf0, 0x28, 0x8c, 0xbc }
                },
                new object[]
                {
                    new byte[] { 0xf0, 0x90, 0x28, 0xbc }
                },
                new object[]
                {
                    new byte[] { 0xf0, 0x28, 0x8c, 0x28 }
                },
                new object[]
                {
                    //CheckOverlong
                    new byte[] { 0xc0, 0x9f }
                },
                new object[]
                {
                    new byte[] { 0xf5, 0xff, 0xff, 0xff }
                },
                new object[]
                {
                    new byte[] { 0xed, 0xa0, 0x81 }
                }
            };

        private IUtf8Validator _validator;

        public BaseValidationTests()
        {
            this._validator = this.CreateValidator();
        }

        private static byte[] CreateGoodSequence()
        {
            string testStringUtf16 = "x≤(α+β)²γ²";
            byte[] testStringUtf16Buffer = Encoding.UTF8.GetBytes(testStringUtf16);
            return testStringUtf16Buffer;
        }

        private static byte[] CreateBadSequence()
        {
            byte[] testStringUtf16Buffer = CreateGoodSequence();

            //create invalid character
            testStringUtf16Buffer[1] = 0;

            return testStringUtf16Buffer;
        }

        protected abstract IUtf8Validator CreateValidator();

        [Theory]
        [MemberData(nameof(_GoodSequences))]
        public void TestGoodSequences(byte[] buffer)
        {
            Assert.True
            (
                this._validator.ValidateUTF8
                (
                    buffer,
                    out uint? position
                )
            );

            Assert.Null(position);
        }

        [Theory]
        [MemberData(nameof(_BadSequences))]
        public void TestBadSequences(byte[] buffer)
        {
            Assert.False
            (
                this._validator.ValidateUTF8
                (
                    buffer,
                    out uint? position
                )
            );

            Assert.NotNull(position);
        }

        [Fact]
        public void TestASCII()
        {
            string testStringAscii = "Hello world!";
            byte[] testStringAsciiBuffer = Encoding.UTF8.GetBytes(testStringAscii);

            Assert.True
            (
                this._validator.ValidateUTF8
                (
                    testStringAsciiBuffer,
                    out uint? position
                )
            );

            Assert.Null(position);
        }
    }
}
