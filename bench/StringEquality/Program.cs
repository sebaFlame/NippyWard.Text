using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NippyWard.Text;

namespace StringEquality
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkRunner.Run<StringEqualityBenchmark>(args: args);
    }

    //[EventPipeProfiler(EventPipeProfile.CpuSampling)]
    public class StringEqualityBenchmark
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(DataUtf16))]
        public bool CoreFXEquality(string strA, string strB)
            => string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public bool FirstDecodeToUtf16Equality(Utf8String strA, Utf8String strB)
        {
            string utf16A = (string)strA;
            string utf16B = (string)strB;

            return string.Equals
            (
                utf16A,
                utf16B,
                StringComparison.OrdinalIgnoreCase
            );
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public bool OrdinalEquals(Utf8String strA, Utf8String strB)
        {
            IEqualityComparer<Utf8String> comparer
                = BaseUtf8StringComparer.Ordinal;
            return comparer.Equals(strA, strB);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public bool OrdinalIgnoreCaseEquals(Utf8String strA, Utf8String strB)
        {
            IEqualityComparer<Utf8String> comparer =
                BaseUtf8StringComparer.OrdinalIgnoreCase;
            return comparer.Equals(strA, strB);
        }

        public static IEnumerable<object[]> DataUtf16()
        {
            yield return new object[] { "CaseFolding", "cASEfOLDING" };
            yield return new object[] { "ЯяЯяЯяЯяЯяЯ", "яЯяЯяЯяЯяЯя" };
        }

        public static IEnumerable<object[]> DataUtf8()
        {
            yield return new object[]
            {
                new Utf8String("CaseFolding"),
                new Utf8String("cASEfOLDING")
            };
            yield return new object[]
            {
                new Utf8String("ЯяЯяЯяЯяЯяЯ"),
                new Utf8String("яЯяЯяЯяЯяЯя")
            };
        }
    }
}
