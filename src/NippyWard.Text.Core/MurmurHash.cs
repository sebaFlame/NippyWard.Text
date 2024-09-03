using System;
using System.Security.Cryptography;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace NippyWard.Text
{
    /* based on
     * https://github.com/odinmillion/MurmurHash.Net/blob/master/src/MurmurHash.Net/MurmurHash3.cs
     */
    public static class MurmurHash
    {
        internal static readonly uint _Seed;

        static MurmurHash()
        {
            Span<byte> seed;

            uint s = 0;

            unsafe
            {
                seed = new Span<byte>((byte*)&s, 4);
            }

            RandomNumberGenerator.Fill(seed);

            _Seed = s;
        }

        public static uint Hash_x86_32(ReadOnlySequence<byte> sequence)
            => Hash_x86_32(sequence, _Seed);

        public static uint Hash_x86_32
        (
            ReadOnlySequence<byte> sequence,
            uint seed
        )
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(sequence);
            int length = (int)sequence.Length;
            uint value;
            uint h1 = seed;

            while(reader.TryPeekUInt(out value))
            {
                h1 = HashUInt(h1, value);
                reader.Advance(4);
            }

            int remainder = (int)reader.Remaining;

            if(remainder > 0)
            {
                uint num = 0;
                int shift = 0;

                while(reader.TryRead(out byte b))
                {
                    num ^= (uint)b << shift;
                    shift += 8;
                }

                h1 ^= RotateLeft(num * 3432918353U, 15) * 461845907U;
            }

            h1 = FinalizeHash(h1, (uint)length);

            return h1;
        }

        internal static uint Hash_x86_32(Span<uint> codePoints)
            => Hash_x86_32(codePoints, _Seed);

        internal static uint Hash_x86_32
        (
            Span<uint> codePoints,
            uint seed
        )
        {
            int length = codePoints.Length;
            uint h1 = seed;

            for(int i = 0; i < codePoints.Length; i++)
            {
            }

            h1 = FMix(h1 ^ (uint)length);

            return h1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint HashUInt(uint hash, uint n)
            => (uint)
            (
                (int)RotateLeft
                (
                    hash ^ RotateLeft
                    (
                        n * 3432918353U,
                        15
                    ) * 461845907U,
                    13
                ) * 5 - 430675100
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint FinalizeHash(uint hash, uint length)
            => FMix(hash ^ length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint RotateLeft(uint x, byte r)
            => x << (int)r | x >> 32 - (int)r;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint FMix(uint h)
        {
            h = (uint)(((int)h ^ (int)(h >> 16)) * -2048144789);
            h = (uint)(((int)h ^ (int)(h >> 13)) * -1028477387);
            return h ^ h >> 16;
        }
    }
}
