using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace NippyWard.Text.Generator
{
    public static class Program
    {
        private static readonly string _CaseFoldingFilePath = @"CaseFolding.txt";

        public static void Main(string[] args)
        {
            BMPGenerator bmpGenerator = new BMPGenerator();
            SMPGenerator smpGenerator = new SMPGenerator();

            string line;
            using(StreamReader sr = new StreamReader(_CaseFoldingFilePath))
            {
                while(!sr.EndOfStream)
                {
                    line = sr.ReadLine().Trim();

                    if(string.IsNullOrEmpty(line) || line.IndexOf('#') == 0)
                    {
                        continue;
                    }

                    string[] parts = line.Split(';');
                    parts[1] = parts[1].Trim();

                    if(parts.Length < 4 || (parts[1] != "C" && parts[1] != "S"))
                    {
                        continue;
                    }

                    if(!int.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int n1) ||
                        !int.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int n2))
                    {
                        continue;
                    }

                    bmpGenerator.ReadCaseFoldCodePoints(n1, n2);
                    smpGenerator.ReadCaseFoldCodePoints(n1, n2);
                }
            }

            using(StreamWriter bmpWriter =
                  new StreamWriter("BMPMapGenerated.cs", false, Encoding.UTF8))
            {
                bmpGenerator.GenerateMap(bmpWriter);
            }
            bmpGenerator.Validate(Console.Out);

            Console.ReadKey();

            using(StreamWriter smpWriter =
                  new StreamWriter("SMPMapGenerated.cs", false, Encoding.UTF8))
            {
                smpGenerator.GenerateMap(smpWriter);
            }
            smpGenerator.Validate(Console.Out);

            Console.ReadKey();
        }
    }
}
