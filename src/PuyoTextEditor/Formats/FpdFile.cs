using PuyoTextEditor.Collections;
using PuyoTextEditor.Resources;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PuyoTextEditor.Formats
{
    public class FpdFile : IFormat
    {
        /// <summary>
        /// Gets the collection of entries that are currently in this file.
        /// </summary>
        public OrderedDictionary<char, FpdEntry> Entries { get; }

        /// <summary>
        /// Gets the characters that are currently in this file.
        /// </summary>
        public List<char> Characters { get; }

        public FpdFile()
        {
            Entries = new OrderedDictionary<char, FpdEntry>();
            Characters = new List<char>();
        }

        public FpdFile(IDictionary<char, FpdEntry> collection)
        {
            Entries = new OrderedDictionary<char, FpdEntry>(collection);
            Characters = new List<char>(collection.Keys);
        }

        public FpdFile(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // FPD files have nothing we can use to identify the file.
                // But their file size will always be a multiple of 3, so we can use that to check.
                if (source.Length % 3 != 0)
                {
                    throw new IOException(string.Format(ErrorMessages.InvalidFpdFile, path));
                }

                Entries = new OrderedDictionary<char, FpdEntry>((int)(source.Length / 3));
                Characters = new List<char>((int)(source.Length / 3));

                while (source.Position < source.Length)
                {
                    var c = reader.ReadChar();
                    var width = reader.ReadByte();

                    if (!Entries.ContainsKey(c))
                    {
                        Entries.Add(c, new FpdEntry
                        {
                            Width = width,
                        });
                    }
                    Characters.Add(c);
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
