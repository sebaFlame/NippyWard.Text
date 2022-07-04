using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

using NippyWard.Text;

namespace NippyWard.Text.Validation
{
    public interface IUtf8Validator
    {
        bool ValidateUTF8
        (
            ReadOnlySpan<byte> buffer,
            [NotNullWhen(false)] out uint? position
        );

        bool ValidateUTF8
        (
            ReadOnlySequence<byte> buffer,
            [NotNullWhen(false)] out SequencePosition? position
        );

        bool ValidateUTF8
        (
            Utf8String @string,
            [NotNullWhen(false)] out SequencePosition? position
        )
            => this.ValidateUTF8(@string.Buffer, out position);
    }
}
