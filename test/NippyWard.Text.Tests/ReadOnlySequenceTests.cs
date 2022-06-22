using System;
using System.Buffers;

using Xunit;

namespace NippyWard.Text.Tests
{
    public class ReadOnlySequenceTests
    {
        private static readonly byte[] _ASCII;

        static ReadOnlySequenceTests()
        {
            _ASCII = new byte[128];

            //fill ascii
            for(int i = 0; i < _ASCII.Length; i++)
            {
                _ASCII[i] = (byte)i;
            }
        }

        [Fact]
        public void SimpleEqualityTest()
        {
            ReadOnlySequence<byte> x
                = new ReadOnlySequence<byte>(new ReadOnlyMemory<byte>(_ASCII));
            ReadOnlySequence<byte> y
                = new ReadOnlySequence<byte>(new ReadOnlyMemory<byte>(_ASCII));

            Assert.True(x.SequenceEquals(y));
        }

        [Fact]
        public void PartsAndFullEqualityTest()
        {
            ReadOnlySequence<byte> x
                = new ReadOnlySequence<byte>(new ReadOnlyMemory<byte>(_ASCII));
            ReadOnlySequence<byte> y = CreateReadOnlySequence(2);

            Assert.True(x.SequenceEquals(y));
        }

        [Fact]
        public void EqualPartsEqualityTest()
        {
            ReadOnlySequence<byte> x = CreateReadOnlySequence(2);
            ReadOnlySequence<byte> y = CreateReadOnlySequence(2);

            Assert.True(x.SequenceEquals(y));
        }

        [Fact]
        public void InequalPartsEqualityTest()
        {
            ReadOnlySequence<byte> x = CreateReadOnlySequence(2);
            ReadOnlySequence<byte> y = CreateReadOnlySequence(4);

            Assert.True(x.SequenceEquals(y));
        }

        [Fact]
        public void SimpleLenghtInequalityTest()
        {
            ReadOnlySequence<byte> x
                = new ReadOnlySequence<byte>(new ReadOnlyMemory<byte>(_ASCII));
            ReadOnlySequence<byte> y
                = new ReadOnlySequence<byte>
                (
                    new ReadOnlyMemory<byte>(_ASCII, 32, 64)
                );

            Assert.False(x.SequenceEquals(y));
        }

        [Fact]
        public void SimpleInequalityTest()
        {
            ReadOnlySequence<byte> x = CreateReadOnlySequence(4);

            byte[] arr = new byte[_ASCII.Length];

            //ensure first part is the same
            for(int i = 0; i < 96; i++)
            {
                arr[i] = (byte)i;
            }

            //then fill with "random"
            for(int i = 96; i < 128; i++)
            {
                arr[i] = (byte)(i - 32);
            }

            ReadOnlySequence<byte> y
                = new ReadOnlySequence<byte>(new ReadOnlyMemory<byte>(arr));

            Assert.False(x.SequenceEquals(y));
        }

        #region helper methods
        private static ReadOnlySequence<byte> CreateReadOnlySequence
        (
            int partLength
        )
        {
            if((_ASCII.Length % partLength) > 0)
            {
                throw new InvalidOperationException
                (
                    $"{_ASCII.Length} not divisable by {partLength}"
                );
            }

            Utf8StringSequenceSegment segment, first = null, previous = null;

            for(int i = 0; i < _ASCII.Length; i += partLength)
            {
                segment = new Utf8StringSequenceSegment
                (
                    new ReadOnlyMemory<byte>(_ASCII, i, partLength)
                );

                if(first is null)
                {
                    first = segment;
                }

                if(previous is not null)
                {
                    previous.AddNext(segment);
                }

                previous = segment;
            }

            return new ReadOnlySequence<byte>
            (
                first,
                0,
                previous,
                partLength
            );
        }
        #endregion
    }
}

