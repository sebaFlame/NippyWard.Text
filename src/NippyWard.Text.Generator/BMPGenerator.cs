using System.IO;
using System.Runtime.CompilerServices;

namespace NippyWard.Text.Generator
{
    public class BMPGenerator : BaseGenerator<ushort>
    {
        private ushort[] _l0;
        private ushort[] _l1;
        private ushort[] _l3;

        public BMPGenerator()
            : base()
        { }

        public override void ReadCaseFoldCodePoints(int cp1, int cp2)
        {
            if(cp1 > 0xFFFF)
            {
                return;
            }

            this.RawTable[(ushort)cp1] = (ushort)cp2;
        }

        public override void GenerateMap(TextWriter textWriter)
        {
            this._l0 = new ushort[0xFF + 1];
            for(ushort i = 0; i <= 0xFF; i++)
            {
                if(this.RawTable.TryGetValue(i, out ushort value))
                {
                    // There is data defined for this codepoint. Use it.
                    this._l0[i] = value;
                }
                else
                {
                    // There is no data defined for this codepoint. Use the default value
                    // specified in the ctor.
                    this._l0[i] = i;
                }
            }

            GenerateTable8_4_4
            (
                ushort.Parse,
                this.RawTable,
                out ushort[] l1,
                out ushort[] l3
            );

            this._l1 = l1;
            this._l3 = l3;

            DumpTableL3
            (
                this._l0,
                "_BMPMapBelowFF",
                textWriter
            );

            DumpTableL1
            (
                this._l1,
                "_BMPMapLevel1",
                textWriter
            );

            DumpTableL3
            (
                this._l3,
                "_BMPMapLevel3",
                textWriter
            );
        }

        public override void Validate(TextWriter textWriter)
        {
            int sizel0 = this._l0.Length * sizeof(ushort);
            int sizel1 = this._l1.Length * sizeof(ushort);
            int sizel3 = this._l3.Length * sizeof(char);

            textWriter.WriteLine($"MapBelow5FF Size     = {sizel0,4}");
            textWriter.WriteLine($"MapBelow5FF Length     = {this._l0.Length,4}");
            textWriter.WriteLine($"MapLevel1 Size     = {sizel1,4}");
            textWriter.WriteLine($"MapLevel1 Length     = {this._l1.Length,4}");
            textWriter.WriteLine($"MapData Size     = {sizel3,4}");
            textWriter.WriteLine($"MapData Length     = {this._l3.Length,4}");
            textWriter.WriteLine($"Total size = {sizel0 + sizel1 + sizel3}");

            // Validate the generated tables

            foreach(ushort kv in this.RawTable.Keys)
            {
                ushort c = this.GetFoldCase(kv);
                if(c != this.RawTable[kv])
                {
                    textWriter.WriteLine($"... {kv:x4}:  {c:x4} != {this.RawTable[kv]:x4}");
                }

                ushort rev = this.GetFoldCase(c);
                if(c != rev)
                {
                    textWriter.WriteLine($"... {c:x4}:  {rev:x4} != {c:x4}");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort GetFoldCase(ushort c)
        {
            if(c <= 0xFF)
            {
                return this._l0[c];
            }

            ushort v = this._l1[c >> 8];
            v = this._l3[v + (c & 0xFF)];

            return v == 0 ? c : v;
        }
    }
}
