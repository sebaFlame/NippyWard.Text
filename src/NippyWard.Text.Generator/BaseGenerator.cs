using System;
using System.Collections.Generic;
using System.IO;

namespace NippyWard.Text.Generator
{
    public abstract class BaseGenerator<T>
        where T : unmanaged
    {
        public Dictionary<ushort, T> RawTable { get; }

        public BaseGenerator()
        {
            this.RawTable = new Dictionary<ushort, T>();
        }

        public abstract void ReadCaseFoldCodePoints(int cp1, int cp2);

        public abstract void GenerateMap(TextWriter textWriter);

        public abstract void Validate(TextWriter textWriter);

        internal static void GenerateTable8_4_4
        (
            Func<string, T> parseString,
            Dictionary<ushort, T> rawData,
            out ushort[] l1,
            out T[] l3
        )
        {
            Dictionary<string, ushort> level3Hash =
                new Dictionary<string, ushort>();

            List<ushort> level1Index = new List<ushort>();
            List<T> level3Data = new List<T>();

            ushort ch = 0;
            ushort valueInHash;

            for(ushort i = 0; i < 256; i++)
            {
                // Generate level 1 indice
                string level3RowData = "";
                for(ushort k = 0; k < 256; k++)
                {
                    //Generate data level values by grouping 256 values 
                    //together. Each element of the 256 value group is
                    //seperated by ";".
                    if(rawData.TryGetValue(ch, out T value))
                    {
                        // There is data defined for this codepoint. Use it.
                        level3RowData = level3RowData + value + ";";
                    }
                    else
                    {
                        //There is no data defined for this codepoint. Use the
                        //default value specified in the ctor.
                        level3RowData = level3RowData + 0 + ";";
                    }

                    ch++;
                }

                // Check if the pattern of these 256 values happens before.
                if(!level3Hash.TryGetValue(level3RowData, out valueInHash))
                {
                    // This is a new group in the data level values.
                    // Get the current count of data level group count for this plane.
                    valueInHash = (ushort)level3Data.Count;

                    // Store this count to the hash table, keyed by the pattern of these 16 values.
                    level3Hash[level3RowData] = valueInHash;

                    // Populate the 256 values into data level data table for this plane.
                    string[] values = level3RowData.Split(';');
                    foreach(string s in values)
                    {
                        if(s.Length > 0)
                        {
                            level3Data.Add(parseString(s));
                        }
                    }

                }

                // Populate the index values into level 1 index table.
                level1Index.Add(valueInHash);
            }

            l1 = level1Index.ToArray();
            l3 = level3Data.ToArray();
        }

        internal static void DumpTableL1
        (
            ushort[] table,
            string name,
            TextWriter textWriter
        )
        {
            const int RawWidth = 16;

            textWriter.Write
            (
            $"private static readonly {typeof(ushort).Name}[] {name} =\n{{\n"
            );

            textWriter.Write($"//");
            for(int i = 0; i < RawWidth; i++)
            {
                textWriter.Write($"{i,6:x}  ");
            }

            textWriter.Write($"\n    0x{table[0]:x4}, ");

            for(int i = 1; i < table.Length; i++)
            {
                textWriter.Write($"0x{table[i]:x4}, ");

                if((i + 1) % RawWidth == 0)
                {
                    textWriter.WriteLine($" // {i - 15:x4} .. {i:x4}");
                    textWriter.Write($"    ");
                }

            }

            textWriter.WriteLine("\n};\n");
        }

        internal static void DumpTableL3
        (
            T[] table,
            string name,
            TextWriter textWriter
        )
        {
            const int RawWidth = 16;

            textWriter.Write
            (
                $"private static readonly {typeof(T).Name}[] {name} =\n{{\n"
            );

            textWriter.Write($"//");
            for(int i = 0; i < RawWidth; i++)
            {
                textWriter.Write($"{i,6:x}  ");
            }

            textWriter.Write($"\n    0x{table[0]:x4}, ");

            for(int i = 1; i < table.Length; i++)
            {
                textWriter.Write($"0x{table[i]:x4}, ");

                if((i + 1) % RawWidth == 0)
                {
                    textWriter.WriteLine($" // {i - 15:x4} .. {i:x4}");
                    textWriter.Write($"    ");
                }
            }

            textWriter.WriteLine("\n};\n");
        }

    }
}