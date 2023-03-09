using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcCardDumpConverter.Models
{
    public class RawDump : NfcCard
    {
        public RawDump(string filePath)
        {
            Array.Copy(File.ReadAllBytes(filePath), RawData, RawData.Length);
            Length = new FileInfo(filePath).Length;
        }

        public override void Write(string filePath) => base.WriteAsRawFormat(filePath);
    }
}
