using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcCardDumpConverter.Models
{
    public class RfidToolsDump : NfcCard
    {
        public RfidToolsDump(string filePath)
        {
            var fileContent = File.ReadAllLines(filePath).Where(l => !l.StartsWith("+Sector")).Select(l => l.Trim());
            long offset = 0;
            foreach (var line in fileContent)
            {
                for (int i = 0; i < line.Length; i += 2)
                {
                    RawData[offset++] = FromDoubleCharToByte(line.Substring(i, 2));
                }
            }
            Length = offset;
        }

        public override void Write(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(fs, Encoding.ASCII);
            int sector = 0;
            for (int block = 0; block < RawData.Length / 16; block++)
            {
                if (block % 4 == 0)
                {
                    sw.Write($"+Sector: {sector++}\n");
                }
                sw.Write(string.Join(string.Empty, RawData[(block * 16)..((block + 1) * 16)].Select(b => FromByteToDoubleChar(b))));
                if (block != RawData.Length / 16 - 1)
                {
                    sw.Write("\n");
                }
            }
        }
    }
}
