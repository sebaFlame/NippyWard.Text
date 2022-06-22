using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace NippyWard.Text
{
    internal class Utf8SimpleCaseFolding : BaseUtf8SimpleCaseFolding
    {
        internal static Utf8SimpleCaseFolding Instance;

        static Utf8SimpleCaseFolding()
        {
            Instance = new Utf8SimpleCaseFolding();
        }

        private Utf8SimpleCaseFolding()
            : base()
        { }
    }
}
