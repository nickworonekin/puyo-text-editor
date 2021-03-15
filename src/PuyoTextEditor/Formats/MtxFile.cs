using PuyoTextEditor.IO;
using PuyoTextEditor.Properties;
using PuyoTextEditor.Serialization;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Formats
{
    public class MtxFile : IFormat
    {
        private readonly MtxEncoding encoding;

        /// <summary>
        /// Gets if this file uses 64-bit integer offsets.
        /// </summary>
        public bool Has64BitOffsets { get; }

        /// <summary>
        /// Gets the collection of entries that are currently in this file.
        /// </summary>
        public List<List<XElement>> Entries { get; }

        public MtxFile(MtxEncoding encoding, bool has64BitOffsets = false)
            : this(null, encoding, has64BitOffsets)
        {
        }

        public MtxFile(IEnumerable<List<XElement>>? collection, MtxEncoding encoding, bool has64BitOffsets = false)
        {
            if (collection is not null)
            {
                Entries = new List<List<XElement>>(collection);
            }
            else
            {
                Entries = new List<List<XElement>>();
            }

            this.encoding = encoding;
            Has64BitOffsets = has64BitOffsets;
        }

        public MtxFile(string path, MtxEncoding encoding)
        {
            this.encoding = encoding;

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // The first 4 bytes in an MTX file tells us its expected file size.
                // This value is incorrect for MTX files in the 15th Anniversary translation.
                // Those files will be identified as being invalid.
                var length = reader.Peek(x => x.ReadInt32());
                if (length != source.Length)
                {
                    throw new FileFormatException(string.Format(Resources.InvalidMtxFile, path));
                }

                Func<BinaryReader, int> ReadInt;
                int intSize;

                // MTX files used on Switch, PS4, Xbox One, and PC store their offsets as 64-bit integers
                // On all other platforms, they are stored as 32-bit integers.
                // We're going to test this by reading the bytes at 0x4 as a 32-bit integer to see if it returns 8.
                // If it doesn't, we'll read bytes at 0x8 as a 64-bit integer to see if it returns 16.
                // If it doesn't, then throw an error.

                // This MTX file uses 32-bit offsets
                if (reader.At(0x4, x => x.ReadInt32()) == 8)
                {
                    Has64BitOffsets = false;
                    intSize = 4;
                    ReadInt = x => x.ReadInt32();
                }

                // This MTX file uses 64-bit offsets
                else if (reader.At(0x8, x => x.ReadInt64()) == 16)
                {
                    Has64BitOffsets = true;
                    intSize = 8;
                    ReadInt = x => (int)x.ReadInt64();
                }

                // Not an MTX file
                else
                {
                    throw new FileFormatException(string.Format(Resources.InvalidMtxFile, path));
                }

                source.Position += intSize;
                var sectionsTablePosition = ReadInt(reader);
                var sectionsCount = (reader.Peek(ReadInt) - sectionsTablePosition) / intSize;

                Entries = new List<List<XElement>>(sectionsCount);
                var offsets = new Queue<int>();

                for (var i = 0; i < sectionsCount; i++)
                {
                    var stringsTablePosition = ReadInt(reader);
                    var stringsCount = (reader.Peek(ReadInt) - stringsTablePosition) / intSize; // OK to do this even if it's the last entry

                    Entries.Add(new List<XElement>(stringsCount));
                }

                foreach (var section in Entries)
                {
                    for (var i = 0; i < section.Capacity; i++)
                    {
                        offsets.Enqueue(ReadInt(reader));
                    }
                }

                foreach (var section in Entries)
                {
                    for (var i = 0; i < section.Capacity; i++)
                    {
                        source.Position = offsets.Dequeue();
                        var s = encoding.Read(reader);
                        section.Add(s);
                    }
                }
            }
        }

        public MtxFile(MtxSerializable mtxSerializable, MtxEncoding encoding)
        {
            this.encoding = encoding;
            Has64BitOffsets = mtxSerializable.Has64BitOffsets;

            Entries = mtxSerializable.Sheets.Select(x =>
                x.Texts.Select(x2 => new XElement("text", x2.Nodes()))
                    .ToList())
                .ToList();
        }

        /// <summary>
        /// Saves this <see cref="MtxFile"/> to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        public void Save(string path)
        {
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                // Determine if we are going to use 32-bit integers or 64-bit integers when storing offsets.
                int intSize;
                Action<int> WriteInt;
                if (Has64BitOffsets)
                {
                    intSize = 8;
                    WriteInt = value => writer.Write((long)value);
                }
                else
                {
                    intSize = 4;
                    WriteInt = writer.Write;
                }

                // Start writing the header
                WriteInt(0); // This will be filled in later
                WriteInt(intSize * 2); // Offset of the section offsets

                // Write out the offsets of the sections & strings
                var position = (int)destination.Position + (Entries.Count * intSize);
                foreach (var section in Entries)
                {
                    WriteInt(position);
                    position += section.Count * intSize;
                }
                foreach (var section in Entries)
                {
                    foreach (var s in section)
                    {
                        WriteInt(position);
                        position += encoding.GetByteCount(s);
                    }
                }

                // Write the strings
                foreach (var section in Entries)
                {
                    foreach (var s in section)
                    {
                        encoding.Write(writer, s);
                    }
                }

                // Go back and fill in the file length
                destination.Position = 0;
                WriteInt(position);
                destination.Position = destination.Length;
            }
        }
    }
}
