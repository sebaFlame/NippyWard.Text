using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NippyWard.Text;

namespace StringHashing
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkRunner.Run<StringHashingBenchmark>(args: args);
    }

    //[EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [MemoryDiagnoser]
    public class StringHashingBenchmark
    {
        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(DataUtf16))]
        public bool Utf16HashingOrdinal(string str)
        {
            HashSet<string> hash = new HashSet<string>()
            {
                str
            };
            return hash.Contains(str);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf16))]
        public bool Utf16HashingIgnoreCase(string str)
        {
            HashSet<string> hash = new HashSet<string>
            (
                StringComparer.OrdinalIgnoreCase
            )
            {
                str
            };
            return hash.Contains(str);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public bool Utf8HashingOrdinal(Utf8String str)
        {
            //use the comparer - hashset general comparer has extra allocation
            HashSet<Utf8String> hash = new HashSet<Utf8String>
            (
                BaseUtf8StringComparer.Ordinal
            )
            {
                str
            };
            return hash.Contains(str);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public bool Utf8HashingIgnoreCase(Utf8String str)
        {
            HashSet<Utf8String> hash = new HashSet<Utf8String>
            (
                BaseUtf8StringComparer.OrdinalIgnoreCase
            )
            {
                str
            };
            return hash.Contains(str);
        }

        public static IEnumerable<string> DataUtf16()
        {
            yield return "CaseFolding";
            yield return "ЯяЯяЯяЯяЯяЯ";
        }

        public static IEnumerable<Utf8String> DataUtf8()
        {
            yield return new Utf8String("CaseFolding");
            yield return new Utf8String("ЯяЯяЯяЯяЯяЯ");
        }
    }
}
