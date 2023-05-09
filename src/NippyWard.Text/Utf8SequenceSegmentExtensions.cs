using System;
using System.Buffers;

namespace NippyWard.Text
{
    public static class Utf8SequenceSegmentExtensions
    {
        public static ReadOnlySequence<byte> CreateReadOnlySequence
        (
            this Utf8StringSequenceSegment start,
            Utf8StringSequenceSegment end
        )
        {
            return new ReadOnlySequence<byte>
            (
                start,
                0,
                end,
                end.Memory.Length //continues until end
            );
        }

        public static Utf8StringSequenceSegment AddNewSequenceSegment
        (
            this Utf8StringSequenceSegment segment,
            in ReadOnlySequence<byte> sequence
        )
        {
            if(sequence.IsEmpty)
            {
                return segment;
            }

            foreach(ReadOnlyMemory<byte> memory in sequence)
            {
                segment = AddNewSequenceSegment
                (
                    segment,
                    memory
                );
            }

            return segment;
        }

        public static Utf8StringSequenceSegment AddNewSequenceSegment
        (
            this Utf8StringSequenceSegment segment,
            ReadOnlyMemory<byte> memory
        )
            => segment.AddNext(new Utf8StringSequenceSegment(memory));
    }
}
