using System.Buffers;
using System;
using System.Collections.Generic;
using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using NippyWard.Text;
using NippyWard.Text.Validation;

namespace Validation
{
    public static class Program
    {
        public static void Main(string[] args)
            => BenchmarkRunner.Run<LineFeedBenchmark>(args: args);
    }

    public class LineFeedBenchmark
    {
        //download these 2 files from https://www.cl.cam.ac.uk/~mgk25/ucs/examples/
        //and put these in the project root
        private const string _Utf8Text = "UTF-8-demo.txt";
        private const string _RevelationText = "revelation.txt";

        private IUtf8Validator _vectorValidation;
        private IUtf8Validator _legacyValidation;

        public static IEnumerable<object> GetTexts()
        {
            yield return new Utf8String(File.ReadAllBytes(_Utf8Text));
            yield return new Utf8String(File.ReadAllBytes(_RevelationText));
        }

        [GlobalSetup]
        public void Setup()
        {
            //resusing these should not be an issue as both texts are valid
            this._vectorValidation = new VectorizedUtf8Validator();
            this._legacyValidation = new LegacyUtf8Validator();
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetTexts))]
        public void LegacyValidation(Utf8String str)
        {
            this._legacyValidation.ValidateUTF8
            (
                str,
                out _
            );
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetTexts))]
        public void VectorizedValidation(Utf8String str)
        {
            this._vectorValidation.ValidateUTF8
            (
                str,
                out _
            );
        }
    }
}