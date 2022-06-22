using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace NippyWard.Text
{
    public ref struct Utf8CodePointEnumerator
    {
        public SequencePosition Position => this._reader.Position;

        private SequenceReader<byte> _reader;
        private uint _remainingCodeUnits;
        private uint _codePoint;

        //cache remaining length
        private int _remaininLength;

        private static readonly byte[] _Lengths =
        {
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 4, 0
        };

        private static readonly byte[] _FirstByteMasks =
        {
            0, 0xFF, 0x1F, 0x0F, 0x07
        };

        private static readonly byte[] _Shiftc =
        {
            0, 18, 12, 6, 0
        };

        public Utf8CodePointEnumerator(ReadOnlySequence<byte> str)
        {
            this._reader = new SequenceReader<byte>(str);
            this._remainingCodeUnits = 0;
            this._codePoint = 0;
            this._remaininLength = 0;
        }

        //return the current code point
        public uint Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this._codePoint;
        }

        public bool MoveNext()
        {
            uint codeUnits;
            int byteLength;
            int remainingLength = default;

            //read from remaining code units
            if(this._remaininLength > 0)
            {
                DecodeCodePoint
                (
                    ref this._codePoint,
                    ref this._remainingCodeUnits,
                    ref this._remaininLength,
                    out _
                );

                return true;
            }
            //try to peek 4 bytes
            else if(this._reader.TryPeekUInt(out codeUnits))
            {
                DecodeCodePoint
                (
                    ref this._codePoint,
                    ref codeUnits,
                    ref remainingLength,
                    out byteLength
                );

                //advance reader with read length
                this._reader.Advance(byteLength);

                return true;
            }
            //try read remaing, and store them
            else if(this._reader.TryReadRemainingUInt
            (
                out this._remainingCodeUnits,
                out this._remaininLength
            ))
            {
                DecodeCodePoint
                (
                    ref this._codePoint,
                    ref this._remainingCodeUnits,
                    ref this._remaininLength,
                    out _
                );

                return true;
            }

            return false;
        }

        private static void DecodeCodePoint
        (
            ref uint codePoint,
            ref uint codeUnits,
            ref int remainingLength,
            out int byteLength
        )
        {
            codePoint = DecodeCodePoint
            (
                codeUnits,
                out byteLength
            );

            //remove processed code units (if end of reader has been
            //reached)
            codeUnits >>= byteLength * 8;
            remainingLength -= byteLength;
        }

        //implemntation from https://nullprogram.com/blog/2017/10/06/
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint DecodeCodePoint
        (
            uint codeUnits,
            out int length
        )
        {
            uint codePoint;
            byte b = (byte)codeUnits;

            //find length of code unit
            length = _Lengths[b >> 3];

            //mask first byte according to length
            codePoint = (uint)(b & _FirstByteMasks[length]) << 18;

            //fill with rest of code unit, masking possible continuation
            //bytes
            codePoint |= (codeUnits & 0x00003f00) << 4;
            codePoint |= (codeUnits & 0x003f0000) >> 10;
            codePoint |= (codeUnits & 0x3f000000) >> 24;

            //shift codepoint according to length, so incorrect continuation
            //bytes fall off
            codePoint >>= _Shiftc[length];

            return codePoint;
        }

    }
}
