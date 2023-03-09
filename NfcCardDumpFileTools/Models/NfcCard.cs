using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcCardDumpConverter.Models
{
    public abstract class NfcCard : IEquatable<NfcCard>
    {
        public byte[] RawData { get; protected set; } = new byte[1024];
        public long Length { get; protected set; }

        public void WriteAsRawFormat(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Create);
            using BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(RawData);
        }

        public abstract void Write(string filePath);

        public static byte FromDoubleCharToByte(string text)
        {
            if (text.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(text), "Text length is not 2");
            }

            checked
            {
                byte result = (byte)(FromSingleHexToByte(text[0]) * 16 + FromSingleHexToByte(text[1]));
                return result;
            }
        }

        public static string FromByteToDoubleChar(byte data) => $"{FromSingleByteToHexChar(checked((byte)(data / 16)))}{FromSingleByteToHexChar(checked((byte)(data % 16)))}";

        public static byte FromSingleHexToByte(char text) => text switch
        {
            (>= '0' and <= '9') => checked((byte)(text - '0')),
            (>= 'A' and <= 'F') => checked((byte)(text - 'A' + 10)),
            (>= 'a' and <= 'f') => checked((byte)(text - 'a' + 10)),
            _ => throw new ArgumentOutOfRangeException(nameof(text), $"Char {text} is out of valid hex values")
        };

        public static char FromSingleByteToHexChar(byte data) => data switch
        {
            (>= 0 and <= 9) => checked((char)(data + '0')),
            (>= 10 and <= 16) => checked((char)(data - 10 + 'A')),
            _ => throw new ArgumentOutOfRangeException(nameof(data), $"Value {data} is out of valid hex values")
        };

        public bool Equals(NfcCard? other)
        {
            if (other == null)
            {
                Console.WriteLine("Compare target does not exist");
                return false;
            }
            if (RawData.Length != other.RawData.Length)
            {
                Console.WriteLine($"Both cards do not equal - {RawData.Length} bytes vs. {other.RawData.Length}");
                return false;
            }
            else
            {
                bool equal = true;
                for (int block = 0; block < RawData.Length / 16; block++)
                {
                    bool[] equality = new bool[16];
                    var self = RawData[(block * 16)..((block + 1) * 16)];
                    var target = other.RawData[(block * 16)..((block + 1) * 16)];
                    equality = self.Select((d, i) => self[i] == target[i]).ToArray();

                    if (equality.Any(e => !e))
                    {
                        Console.WriteLine($"Sector {block / 4} Block {block % 4} are not equal");
                        equal = false;

                        Console.Write("S: ");
                        for (int i = 0; i < 16; i++)
                        {
                            if (!equality[i])
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                            }
                            Console.Write(FromByteToDoubleChar(self[i]));
                            Console.Write(" ");
                            if (!equality[i])
                            {
                                Console.ResetColor();
                            }
                        }
                        Console.WriteLine();

                        Console.Write("T: ");
                        for (int i = 0; i < 16; i++)
                        {
                            if (!equality[i])
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                            }
                            Console.Write(FromByteToDoubleChar(target[i]));
                            Console.Write(" ");
                            if (!equality[i])
                            {
                                Console.ResetColor();
                            }
                        }
                        Console.WriteLine();
                    }
                }

                if (equal)
                {
                    Console.WriteLine("Both cards are identical");
                }

                return equal;
            }
        }
    }
}
