using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace NippyWard.Text
{
    public ref struct Utf8CodePointEnumerator
    {
        public long Index => this._reader.Consumed - this._cpLength;
        public long Position => this._reader.Consumed;

        private SequenceReader<byte> _reader;
        private uint _remainingCodeUnits;
        private uint _codePoint;
        private int _cpLength;

        //cache remaining length
        private int _remainingLength;

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
            this._remainingLength = 0;
            this._cpLength = 0;
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
            if(this._remainingLength > 0)
            {
                DecodeCodePoint
                (
                    ref this._codePoint,
                    ref this._remainingCodeUnits,
                    ref this._remainingLength,
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

                //set the correct length to calculate position
                this._cpLength = byteLength;

                return true;
            }
            //try read remaing, and store them
            else if(this._reader.TryReadRemainingUInt
            (
                out this._remainingCodeUnits,
                out this._remainingLength
            ))
            {
                DecodeCodePoint
                (
                    ref this._codePoint,
                    ref this._remainingCodeUnits,
                    ref this._remainingLength,
                    out _
                );

                return true;
            }

            return false;
        }

        public uint Last
        {
            get
            {
                if (this._reader.Length >= 4)
                {
                    this._reader.AdvanceToEnd();
                    this._reader.Rewind(4);
                }

                uint cp = 0;
                uint remainingCodeUnits;
                int remainingLength;

                //read the last 4 bytes and decode 1 by 1 till the last one
                if (this._reader.TryReadRemainingUInt
                (
                    out remainingCodeUnits,
                    out remainingLength
                ))
                {
                    do
                    {
                        DecodeCodePoint
                        (
                            ref cp,
                            ref remainingCodeUnits,
                            ref remainingLength,
                            out _
                        );
                    } while (remainingLength > 0);
                }

                return cp;
            }
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
