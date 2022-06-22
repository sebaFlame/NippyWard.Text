using System;
using System.Buffers;

namespace NippyWard.Text
{
    public class Utf8StringSequenceSegment : ReadOnlySequenceSegment<byte>
    {
        public Utf8StringSequenceSegment(ReadOnlyMemory<byte> message)
        {
            this.Memory = message;
            this.RunningIndex = 0;
        }

        public Utf8StringSequenceSegment(string message)
            : this(Utf8String.FromUtf16(message))
        { }

        public Utf8StringSequenceSegment AddNext(Utf8StringSequenceSegment next)
        {
            if(next is null)
            {
                return next;
            }

            this.Next = next;
            next.RunningIndex = this.RunningIndex + this.Memory.Length;
            return next;
        }

        public void Reset()
        {
            ReadOnlySequenceSegment<byte> next = this.Next;

            this.Next = null;
            this.RunningIndex = 0;

            if(next is Utf8StringSequenceSegment segment)
            {
                segment.Reset();
            }
        }
    }
}
