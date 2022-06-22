using System;
using System.Buffers;

using Xunit;

using NippyWard.Text;
using NippyWard.Text.LineFeed;

namespace NippyWard.Text.Tests
{
    public class LineFeedTests
    {
        ILineFeed _line;

        public LineFeedTests()
        {
            this._line = new LineFeed.LineFeed();
        }

        [Fact]
        public void LineFeed_16Byte_Test()
        {
            //ensure this is longer than 16 bytes (in ASCII)
            Utf8String str = new Utf8String("Hello World, Hello Universe\n");
            ReadOnlySequence<byte> buffer = str.Buffer;

            Assert.True
            (
                this._line.TryGetLineFeed(in buffer, out SequencePosition lf)
            );

            long pos = buffer.GetOffset(lf);
            Assert.Equal(buffer.Length - 1, pos);
        }

        [Fact]
        public void LineFeed_8Byte_Test()
        {
            //ensure this is longer than 16 bytes (in ASCII)
            Utf8String str = new Utf8String("Hello World\n");
            ReadOnlySequence<byte> buffer = str.Buffer;

            Assert.True
            (
                this._line.TryGetLineFeed(in buffer, out SequencePosition lf)
            );

            long pos = buffer.GetOffset(lf);
            Assert.Equal(buffer.Length - 1, pos);
        }
    }
}
