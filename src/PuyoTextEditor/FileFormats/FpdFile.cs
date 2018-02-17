using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class FpdFile : List<FpdEntry>
    {
        public FpdFile()
            : base()
        { }

        public FpdFile(IEnumerable<FpdEntry> collection)
            : base(collection)
        { }

        public FpdFile(int capacity)
            : base(capacity)
        { }

        public static FpdFile Read(string path)
        {
            var fpdFile = new FpdFile();

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // FPD files have nothing we can use to identify the file.
                // But their file size will always be a multiple of 3, so we can use that to check.
                if (source.Length % 3 != 0)
                {
                    throw new IOException($"{path} is not a valid FPD file.");
                }

                while (source.Position < source.Length)
                {
                    fpdFile.Add(new FpdEntry
                    {
                        Character = reader.ReadChar(),
                        Width = reader.ReadByte(),
                    });

                    source.Position++;
                }
            }

            return fpdFile;
        }

        public void Write(string path)
        {
            // Sort the entries
            var sortedEntries = this.OrderBy(x => x.Character);

            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                foreach (var entry in sortedEntries)
                {
                    writer.Write(entry.Character);
                    writer.Write(entry.Width);
                }
            }
        }
    }
}
