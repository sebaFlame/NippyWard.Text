using System.IO;

namespace NippyWard.Text.Generator
{
    public class SMPGenerator : BaseGenerator<ushort>
    {
        private ushort[] _l1;
        private ushort[] _l3;

        public SMPGenerator()
            : base()
        { }

        public override void ReadCaseFoldCodePoints(int cp1, int cp2)
        {
            if(cp1 <= 0xFFFF)
            {
                return;
            }

            cp1 -= 0x010000;

            //all values are below 01FFFF, can use a short
            this.RawTable[(ushort)cp1] = (ushort)cp2;
        }

        public override void GenerateMap(TextWriter textWriter)
        {
            GenerateTable8_4_4
            (
                ushort.Parse,
                this.RawTable,
                out ushort[] l1,
                out ushort[] l3
            );

            this._l1 = l1;
            this._l3 = l3;

            DumpTableL1
            (
                this._l1,
                "_SMPMapLevel1",
                textWriter
            );

            DumpTableL3
            (
                this._l3,
                "_SMPMapLevel3",
                textWriter
            );
        }

        public override void Validate(TextWriter textWriter)
        {
            int sizel1 = this._l1.Length * sizeof(ushort);
            int sizel3 = this._l3.Length * sizeof(ushort);

            textWriter.WriteLine($"MapSurrogateLevel1 Length     = {this._l1.Length}");
            textWriter.WriteLine($"MapSurrogateLevel1 Size       = {sizel1}");
            textWriter.WriteLine($"MapSurrogateData Length   = {this._l3.Length}");
            textWriter.WriteLine($"MapSurrogateData Size     = {sizel3}");
            textWriter.WriteLine($"Total size               = {sizel1 + sizel3}");

            // Validate the generated tables
            foreach(ushort kv in this.RawTable.Keys)
            {
                ushort c = this.GetFoldCase(kv);
                if(c != this.RawTable[kv])
                {
                    textWriter.WriteLine($"... {kv:x4}:  {c:x4} != {this.RawTable[kv]:x4}");
                }

                //also try the reverse
                ushort rev = this.GetFoldCase(c);
                if(c != rev)
                {
                    textWriter.WriteLine($"... {c:x4}:  {rev:x4} != {c:x4}");
                }
            }
        }

        private ushort GetFoldCase(ushort c)
        {
            ushort v = this._l1[c >> 8];
            v = this._l3[v + (c & 0xFF)];

            return v == 0 ? c : v;
        }
    }
}
