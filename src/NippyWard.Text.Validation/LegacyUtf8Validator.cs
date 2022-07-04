using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippyWard.Text.Validation
{
    public class LegacyUtf8Validator : IUtf8Validator
    {
        private Decoder _decoder;

        private static Encoding _Utf8;

        static LegacyUtf8Validator()
        {
            _Utf8 = new UTF8Encoding(false, true);
        }

        public LegacyUtf8Validator()
        {
            this._decoder = _Utf8.GetDecoder();
        }

        private static bool ValidateUTF8Core
        (
            in ReadOnlySpan<byte> buffer,
            Decoder decoder,
            [NotNullWhen(false)] out uint? position
        )
        {
            try
            {
                decoder.GetCharCount
                (
                    buffer,
                    false
                );

                position = null;
                return true;
            }
            catch(DecoderFallbackException ex)
            {
                position = (uint)ex.Index;
                return false;
            }
        }

        public bool ValidateUTF8
        (
            ReadOnlySpan<byte> buffer,
            [NotNullWhen(false)] out uint? position
        )
            => ValidateUTF8Core
            (
                in buffer,
                this._decoder,
                out position
            );

        public bool ValidateUTF8
        (
            ReadOnlySequence<byte> buffer,
            [NotNullWhen(false)] out SequencePosition? position
        )
        {
            ReadOnlySpan<byte> s;
            uint? iPos;

            if (buffer.IsSingleSegment)
            {
                s = buffer.FirstSpan;

                if(!ValidateUTF8Core
                (
                    s,
                    this._decoder,
                    out iPos
                ))
                {
                    position = buffer.GetPosition(iPos.Value);
                    return false;
                }

                position = null;
                return true;
            }

            ReadOnlySequence<byte>.Enumerator enumerator
                = buffer.GetEnumerator();
            ReadOnlyMemory<byte> m;
            uint length = 0;

            while (enumerator.MoveNext())
            {
                m = enumerator.Current;
                s = m.Span;

                if (s.IsEmpty)
                {
                    continue;
                }

                if (!ValidateUTF8Core
                (
                        s,
                        this._decoder,
                        out iPos
                ))
                {
                    position = buffer.GetPosition(length + iPos.Value);
                    return false;
                }

                length += (uint)s.Length;
            }

            position = null;
            return true;
        }
    }
}
