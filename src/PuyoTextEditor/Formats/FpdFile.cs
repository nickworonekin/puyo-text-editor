using PuyoTextEditor.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PuyoTextEditor.Formats
{
    public class FpdFile : IFormat
    {
        public OrderedDictionary<char, FpdEntry> Entries { get; }

        public FpdFile()
        {
            Entries = new OrderedDictionary<char, FpdEntry>();
        }

        public FpdFile(IDictionary<char, FpdEntry> collection)
        {
            Entries = new OrderedDictionary<char, FpdEntry>(collection);
        }

        public FpdFile(string path)
        {
            Entries = new OrderedDictionary<char, FpdEntry>();

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
                    var c = reader.ReadChar();
                    var width = reader.ReadByte();

                    Entries.Add(c, new FpdEntry
                    {
                        Width = width,
                    });
                }
            }
        }

        /// <summary>
        /// Saves this <see cref="FpdFile"/> to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        public void Save(string path)
        {
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                foreach (var entry in Entries)
                {
                    writer.Write(entry.Key);
                    writer.Write(entry.Value.Width);
                }
            }
        }
    }
}
