using System.Buffers;
using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NippyWard.Text;
using NippyWard.Text.LineFeed;

namespace LineFeed
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkRunner.Run<LineFeedBenchmark>(args: args);
    }

    public class LineFeedBenchmark
    {
        private ILineFeed _vectorLineFeed;
        private ILineFeed _longLineFeed;

        public static IEnumerable<object> DataUtf8()
        {
            yield return new Utf8String("Hello World\n");
            yield return new Utf8String(string.Concat(new string('a', 256), '\n'));
        }

        [GlobalSetup]
        public void Setup()
        {
            this._vectorLineFeed = new VectorLineFeed();
            this._longLineFeed = new LongLineFeed();
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(DataUtf8))]
        public void CoreFxPosition(Utf8String str)
        {
            SequencePosition? pos = str.Buffer.PositionOf<byte>(0x0A);
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public void VectorPosition(Utf8String str)
        {
            this._vectorLineFeed.TryGetLineFeed
            (
                str,
                out SequencePosition pos
            );
        }

        [Benchmark]
        [ArgumentsSource(nameof(DataUtf8))]
        public void LongPosition(Utf8String str)
        {
            this._longLineFeed.TryGetLineFeed
            (
                str,
                out SequencePosition pos
            );
        }
    }
}