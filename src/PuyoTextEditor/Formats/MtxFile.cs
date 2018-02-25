using PuyoTextEditor.IO;
using PuyoTextEditor.Resources;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public List<List<string>> Entries { get; }

        public MtxFile(MtxEncoding encoding, bool has64BitOffsets = false)
        {
            this.encoding = encoding;
            Has64BitOffsets = has64BitOffsets;
            Entries = new List<List<string>>();
        }

        public MtxFile(IEnumerable<List<string>> collection, MtxEncoding encoding, bool has64BitOffsets = false)
        {
            this.encoding = encoding;
            Has64BitOffsets = has64BitOffsets;
            Entries = new List<List<string>>(collection);
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
                var length = reader.PeekInt32();
                if (length != source.Length)
                {
                    throw new IOException(string.Format(ErrorMessages.InvalidMtxFile, path));
                }

                Func<int> ReadInt, PeekInt;
                int intSize;

                // In the Switch version of Puyo Puyo Tetris, offsets are stored as 64-bit integers
                // In all other versions, they are stored as 32-bit integers.
                // We're going to test this by reading bytes 4-8 as a 32-bit integer to see if it returns 8.
                // If it doesn't, we'll read bytes 8-16 as a 64-bit integer to see if it returns 16.
                // If it doesn't, then throw an error.
                source.Position += 4;
                var testInt32 = reader.PeekInt32();
                source.Position -= 4;

                if (testInt32 == 8)
                {
                    // This MTX file uses 32-bit offsets
                    Has64BitOffsets = false;
                    intSize = 4;
                    ReadInt = reader.ReadInt32;
                    PeekInt = reader.PeekInt32;
                }
                else
                {
                    source.Position += 8;
                    var testInt64 = reader.PeekInt64();
                    source.Position -= 8;

                    if (testInt64 == 16)
                    {
                        // This MTX file uses 64-bit offsets
                        Has64BitOffsets = true;
                        intSize = 8;
                        ReadInt = () => (int)reader.ReadInt64();
                        PeekInt = () => (int)reader.PeekInt64();
                    }

                    else
                    {
                        throw new IOException(string.Format(ErrorMessages.InvalidMtxFile, path));
                    }
                }

                source.Position += intSize;
                var sectionsTablePosition = ReadInt();
                var sectionsCount = (PeekInt() - sectionsTablePosition) / intSize;

                Entries = new List<List<string>>(sectionsCount);
                var offsets = new Queue<int>();

                for (var i = 0; i < sectionsCount; i++)
                {
                    var stringsTablePosition = ReadInt();
                    var stringsCount = (PeekInt() - stringsTablePosition) / intSize; // OK to do this even if it's the last entry

                    Entries.Add(new List<string>(stringsCount));
                }

                foreach (var section in Entries)
                {
                    for (var i = 0; i < section.Capacity; i++)
                    {
                        offsets.Enqueue(ReadInt());
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
