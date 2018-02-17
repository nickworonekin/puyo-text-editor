using PuyoTextEditor.IO;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class MtxFile : List<List<string>>
    {
        private readonly MtxEncoding encoding;
        private readonly bool has64BitOffsets;

        public MtxFile(MtxEncoding encoding, bool has64BitOffsets = false)
            : base()
        {
            this.encoding = encoding;
            this.has64BitOffsets = has64BitOffsets;
        }

        public MtxFile(List<List<string>> collection, MtxEncoding encoding, bool has64BitOffsets = false)
            : base(collection)
        {
            this.encoding = encoding;
            this.has64BitOffsets = has64BitOffsets;
        }

        public MtxFile(int capacity, MtxEncoding encoding, bool has64BitOffsets = false)
            : base(capacity)
        {
            this.encoding = encoding;
            this.has64BitOffsets = has64BitOffsets;
        }

        public static MtxFile Read(string path, MtxEncoding encoding)
        {
            MtxFile mtxFile;

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // The first 4 bytes in an MTX file tells us its expected file size.
                // This value is incorrect for MTX files in the 15th Anniversary translation.
                // Those files will be identified as being invalid.
                var length = reader.PeekInt32();
                if (length != source.Length)
                {
                    throw new IOException($"{path} is not a valid MTX file.");
                }

                Func<int> ReadInt, PeekInt;
                int intSize;

                // In the Switch version of Puyo Puyo Tetris, offsets are stored as 64-bit integers
                // In all other versions, they are stored as 32-bit integers.
                // We're going to read bytes 4-8 to determine the size of the integers
                source.Position += 4;
                var testInteger = reader.PeekInt32();
                source.Position -= 4;

                if (testInteger == 0)
                {
                    // testInteger is 0, assume this is a 64-bit integer
                    mtxFile = new MtxFile(encoding, true);
                    intSize = 8;
                    ReadInt = () => (int)reader.ReadInt64();
                    PeekInt = () => (int)reader.PeekInt64();
                }
                else
                {
                    // testInteger is not 0, assume this is a 32-bit integer
                    mtxFile = new MtxFile(encoding, false);
                    intSize = 4;
                    ReadInt = () => reader.ReadInt32();
                    PeekInt = () => reader.PeekInt32();
                }

                source.Position += intSize;
                var sectionsTablePosition = ReadInt();
                source.Position = sectionsTablePosition;
                var sectionsCount = (PeekInt() - sectionsTablePosition) / intSize;

                var sections = new List<MtxSection>(sectionsCount);

                for (var i = 0; i < sectionsCount; i++)
                {
                    var stringsTablePosition = ReadInt();
                    var stringsCount = (PeekInt() - stringsTablePosition) / intSize; // OK to do this even if it's the last entry

                    sections.Add(new MtxSection
                    {
                        Position = stringsTablePosition,
                        Strings = new List<MtxString>(stringsCount),
                        StringsCount = stringsCount,
                    });
                }

                foreach (var section in sections)
                {
                    for (var i = 0; i < section.StringsCount; i++)
                    {
                        section.Strings.Add(new MtxString
                        {
                            Position = ReadInt(),
                        });
                    }
                }

                foreach (var section in sections)
                {
                    var currentSection = new List<string>(section.StringsCount);
                    mtxFile.Add(currentSection);

                    foreach (var @string in section.Strings)
                    {
                        if (@string.Position > source.Length)
                        {
                            @string.String = string.Empty;
                            continue;
                        }

                        source.Position = @string.Position;
                        @string.String = encoding.Read(reader);
                        currentSection.Add(@string.String);
                    }
                }

                return mtxFile;
            }
        }

        public void Write(string path)
        {
            // Determine if we are going to use 32-bit integers or 64-bit integers when storing offsets.
            int intSize;
            if (has64BitOffsets)
            {
                intSize = 8;
            }
            else
            {
                intSize = 4;
            }

            var sections = new List<MtxSection>();

            var position = 8 + (Count * intSize);
            foreach (var jsonSection in this)
            {
                var section = new MtxSection
                {
                    Position = position,
                    Strings = jsonSection.Select(x => new MtxString
                    {
                        String = x,
                    })
                    .ToList(),
                };
                sections.Add(section);

                position += jsonSection.Count * intSize;
            }

            foreach (var section in sections)
            {
                foreach (var @string in section.Strings)
                {
                    @string.Position = position;
                    position += encoding.GetByteCount(@string.String);
                }
            }

            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                Action<int> WriteInt;
                if (has64BitOffsets)
                {
                    WriteInt = value => writer.Write((long)value);
                }
                else
                {
                    WriteInt = value => writer.Write(value);
                }

                WriteInt(position);
                WriteInt(8);

                foreach (var section in sections)
                {
                    WriteInt(section.Position);
                }

                foreach (var section in sections)
                {
                    foreach (var @string in section.Strings)
                    {
                        WriteInt(@string.Position);
                    }
                }

                foreach (var section in sections)
                {
                    foreach (var @string in section.Strings)
                    {
                        encoding.Write(writer, @string.String);
                    }
                }
            }
        }
    }
}
