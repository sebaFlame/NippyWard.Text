using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NippyWard.Text;

namespace StringComparer
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkRunner.Run<StringComparerBenchmark>(args: args);
    }

    //[EventPipeProfiler(EventPipeProfile.CpuSampling)]
    public class StringComparerBenchmark
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(DataUtf16))]
        public int CoreFXCompare(string strA, string strB)
            => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public int FirstDecodeToUtf16Compare(Utf8String strA, Utf8String strB)
        {
            string utf16A = (string)strA;
            string utf16B = (string)strB;

            return string.Compare
            (
                utf16A,
                utf16B,
                StringComparison.OrdinalIgnoreCase
            );
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public int OrdinalCompare(Utf8String strA, Utf8String strB)
        {
            IComparer<Utf8String> comparer = BaseUtf8StringComparer.Ordinal;
            return comparer.Compare(strA, strB);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public int OrdinalIgnoreCaseCompare(Utf8String strA, Utf8String strB)
        {
            IComparer<Utf8String> comparer =
                BaseUtf8StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(strA, strB);
        }

        public static IEnumerable<object[]> DataUtf16()
        {
            yield return new object[] { "CaseFolding1", "cASEfOLDING2" };
            yield return new object[] { "ЯяЯяЯяЯяЯяЯ1", "яЯяЯяЯяЯяЯя2" };
        }

        public static IEnumerable<object[]> DataUtf8()
        {
            yield return new object[]
            {
                new Utf8String("CaseFolding1"),
                new Utf8String("cASEfOLDING2")
            };
            yield return new object[]
            {
                new Utf8String("ЯяЯяЯяЯяЯяЯ1"),
                new Utf8String("яЯяЯяЯяЯяЯя2")
            };
        }
    }
}
