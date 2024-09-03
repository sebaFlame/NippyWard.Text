using System;
using System.Buffers;

namespace NippyWard.Text
{
    public static class ReadOnlySequenceExtensions
    {
        //byte per byte equality check
        public static bool SequenceEquals
        (
            this ReadOnlySequence<byte> x,
            ReadOnlySequence<byte> y
        )
        {
            if(x.Length != y.Length)
            {
                return false;
            }

            ReadOnlySequence<byte>.Enumerator xE = x.GetEnumerator();
            ReadOnlySequence<byte>.Enumerator yE = y.GetEnumerator();
            ReadOnlyMemory<byte> xM = default, yM = default, xC = default,
                yC = default;
            int length;
            ReadOnlySpan<byte> xS, yS;

            while((xM.IsEmpty && xE.MoveNext())
                | (yM.IsEmpty && yE.MoveNext()))
            {
                if(xM.IsEmpty)
                {
                    xM = xC = xE.Current;
                }

                if(yM.IsEmpty)
                {
                    yM = yC = yE.Current;
                }

                length = Math.Min(xC.Length, yC.Length);

                //get minimum comparison length
                xC = xM.Slice(0, length);
                yC = yM.Slice(0, length);

                xS = xC.Span;
                yS = yC.Span;

                if(!xS.SequenceEqual(yS))
                {
                    return false;
                }

                //slice original
                yM = yM.Slice(length);
                xM = xM.Slice(length);
            }

            //equal when both are at end
            return xM.IsEmpty
                && yM.IsEmpty
                && !(xE.MoveNext()
                    || yE.MoveNext());
        }
    }
}

