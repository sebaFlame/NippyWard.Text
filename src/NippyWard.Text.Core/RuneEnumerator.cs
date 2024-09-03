using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NippyWard.Text
{
    internal ref struct RuneEnumerator
    {
        private Utf8String _str;
        private Utf8CodePointEnumerator _enumerator;

        public RuneEnumerator(Utf8String str)
        {
            this._str = str;
            this._enumerator = str.GetEnumerator();
        }

        public Rune Current => new Rune(this._enumerator.Current);

        public bool MoveNext()
        {
            return this._enumerator.MoveNext();
        }

        public void Reset()
        {
            this._enumerator = this._str.GetEnumerator();
        }

        public void Dispose()
        {
            //NOP
        }
    }
}
