using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StatEdit
{
    class StatTable
    {
        public static readonly int NUMBER_OF_CLASSES = 12;

        // includes that weird level 0 thing
        public static readonly int LEVELS_PER_CLASS = 100;
        public StatLevel[,] Entries { get; set; }

        public StatTable(string fileName)
        {
            Entries = new StatLevel[NUMBER_OF_CLASSES, LEVELS_PER_CLASS];
            var statTableBytes = File.ReadAllBytes(fileName);

            for (int i = 0; i < NUMBER_OF_CLASSES; i++)
            {
                for (int j = 0; j < LEVELS_PER_CLASS; j++)
                {
                    var arrivalPoint = (i * StatLevel.EXPECTED_LENGTH * LEVELS_PER_CLASS) +
                                (j * StatLevel.EXPECTED_LENGTH);

                    Entries[i, j] = new StatLevel(
                        statTableBytes.Skip(arrivalPoint)
                        .Take(StatLevel.EXPECTED_LENGTH)
                        .ToArray());
                }
            }
        }

        public void WriteToFile(string fileName)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (var entry in Entries) { writer.Write(entry.ToByteArray()); }
                File.WriteAllBytes(fileName, stream.ToArray());
            }
        }
    }

    class StatLevel
    {
        public static readonly int EXPECTED_LENGTH = 0x14;
        public uint hp { get; set; }
        public uint tp { get; set; }

        public short str { get; set; }
        public short vit { get; set; }
        public short agi { get; set; }
        public short luc { get; set; }
        public short tec { get; set; }

        // might as well!
        short wis;

        public StatLevel(byte[] data)
        {
            hp = BitConverter.ToUInt32(data, 0x0);
            tp = BitConverter.ToUInt32(data, 0x4);
            str = data[0x8];
            vit = data[0xA];
            agi = data[0xC];
            luc = data[0xE];
            tec = data[0x10];
            wis = data[0x12];
        }

        public uint[] GetStatArray()
        {
            var toReturn = new List<uint>();

            toReturn.Add(hp);
            toReturn.Add(tp);
            toReturn.Add((uint) str);
            toReturn.Add((uint) tec);
            toReturn.Add((uint) vit);
            toReturn.Add((uint) agi);
            toReturn.Add((uint) luc);

            return toReturn.ToArray();
        }

        public byte[] ToByteArray()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(hp);
                writer.Write(tp);
                writer.Write(str);
                writer.Write(vit);
                writer.Write(agi);
                writer.Write(luc);
                writer.Write(tec);
                writer.Write(wis);

                return stream.ToArray();
            }
        }
    }
}
