# NippyWard.IRC.Parser
A high-performance UTF-8 implementation for .NET 

## Rationale
I needed an UTF-8 wrapper for different projects. This is a light-weight wrapper around `ReadOnlySequence<byte>`.

The UTF-8 code is based on:
- The experimental [System.Text.Utf8String](https://github.com/dotnet/corefxlab/tree/archive/src/System.Text.Utf8String)
- The experimental [System.Text.CaseFolding](https://github.com/dotnet/corefxlab/tree/archive/src/System.Text.CaseFolding)
- [fastvalidate-utf-8](https://github.com/lemire/fastvalidate-utf-8), a fast SIMD accelerated UTF-8 validator.

## Installation
Clone the repository.

## Usage
Use `Utf8String` to wrap UTF-8 byte sequences.

Use `LegacyUtf8Validator` or `VectorizedUtf8Validator` to validate those sequences.

Use `LegacyLineFeed` or `VectorLineFeed` to check for line feeds in those sequence.

Benchmarks and tests are included if you need usage examples!