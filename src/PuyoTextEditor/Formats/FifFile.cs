using PuyoTextEditor.Collections;
using PuyoTextEditor.IO;
using PuyoTextEditor.Properties;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PuyoTextEditor.Formats
{
    public class FifFile : IFormat
    {
        /// <summary>
        /// Gets if the byte order ("endianness") in which data is stored is big-endian.
        /// </summary>
        public bool IsBigEndian { get; }

        /// <summary>
        /// Get the number of characters in each font texture.
        /// </summary>
        public int CharactersPerTexture { get; set; }

        /// <summary>
        /// Gets the width of each font texture.
        /// </summary>
        public short Width { get; }

        /// <summary>
        /// Gets the height of each font texture.
        /// </summary>
        public short Height { get; }

        /// <summary>
        /// Gets the width of the last font texture.
        /// </summary>
        /// <remarks>This will be equal to <see cref="Width"/> when <see cref="TextureCount"/> is 1.</remarks>
        public short LastWidth { get; }

        /// <summary>
        /// Gets the height of the last font texture.
        /// </summary>
        /// <remarks>This will be equal to <see cref="Height"/> when <see cref="TextureCount"/> is 1.</remarks>
        public short LastHeight { get; }

        /// <summary>
        /// Gets the number of font textures.
        /// </summary>
        public short TextureCount { get; }

        /// <summary>
        /// Gets the number of characters per row.
        /// </summary>
        public short CharactersPerRow { get; }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        public short RowCount { get; }

        /// <summary>
        /// Gets the width of each character.
        /// </summary>
        public short CharacterWidth { get; }

        /// <summary>
        /// Gets the height of each character.
        /// </summary>
        public short CharacterHeight { get; }

        /// <summary>
        /// Gets the width of each column.
        /// </summary>
        public short ColumnWidth { get; }

        /// <summary>
        /// Gets the height of each row.
        /// </summary>
        public short RowHeight { get; }

        /// <summary>
        /// Gets the collection of entries.
        /// </summary>
        public OrderedDictionary<char, FifEntry> Entries { get; }

        public FifFile(short width, short height, short characterWidth, short characterHeight, bool isBigEndian = false)
            : this(null, width, height, characterWidth, characterHeight, isBigEndian)
        {
        }

        public FifFile(IDictionary<char, FifEntry>? collection, short width, short height, short characterWidth, short characterHeight, bool isBigEndian = false)
        {
            if (collection is not null)
            {
                Entries = new OrderedDictionary<char, FifEntry>(collection);
            }
            else
            {
                Entries = new OrderedDictionary<char, FifEntry>();
            }

            // Make sure the character width/height (with expected padding) aren't larger than the width/height
            if (characterWidth + 2 > width)
            {
                throw new ArgumentException(string.Format(Resources.CannotBeLargerThan, nameof(characterWidth), nameof(width)));
            }
            if (characterHeight + 2 > height)
            {
                throw new ArgumentException(string.Format(Resources.CannotBeLargerThan, nameof(characterHeight), nameof(height)));
            }

            Width = width;
            Height = height;
            CharacterWidth = characterWidth;
            CharacterHeight = characterHeight;

            // Calculate the other properties based on the parameters passed
            LastWidth = width;
            LastHeight = height;
            ColumnWidth = (short)(characterWidth + 2);
            RowHeight = (short)(characterHeight + 2);
            CharactersPerRow = (short)(width / characterWidth);
            RowCount = (short)(height / characterHeight);
            CharactersPerTexture = CharactersPerRow * RowCount;
        }

        public FifFile(string path)
        {
            Entries = new OrderedDictionary<char, FifEntry>();

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // FNT files start with the magic code FONTDATF
                if (!(reader.ReadByte() == 'F' && reader.ReadByte() == 'O' && reader.ReadByte() == 'N' && reader.ReadByte() == 'T'
                    && reader.ReadByte() == 'D' && reader.ReadByte() == 'A' && reader.ReadByte() == 'T' && reader.ReadByte() == 'F'))
                {
                    throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                }

                // The next 4 bytes tell us the endianess of the file.
                // 1 if this file is big-endian
                // 0 if this file is little-endian
                IsBigEndian = reader.ReadInt32() == 1;

                Func<char> ReadChar;
                Func<short> ReadInt16;
                Func<int> ReadInt32;

                if (IsBigEndian)
                {
                    ReadChar = () => (char)BinaryPrimitives.ReverseEndianness(reader.ReadChar());
                    ReadInt16 = () => BinaryPrimitives.ReverseEndianness(reader.ReadInt16());
                    ReadInt32 = () => BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
                }
                else
                {
                    ReadChar = reader.ReadChar;
                    ReadInt16 = reader.ReadInt16;
                    ReadInt32 = reader.ReadInt32;
                }

                // The next 16-bit integer appears to always be 101 (0x65)
                if (ReadInt16() != 101)
                {
                    throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                }

                var entryTableOffset = ReadInt16(); // This is usually always 56
                var entryCount = ReadInt32();

                // Check the file size and make sure it is expected size
                if (source.Length != entryTableOffset + (entryCount * 16))
                {
                    throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                }

                CharactersPerTexture = ReadInt32();
                TextureCount = ReadInt16();
                Width = ReadInt16();
                Height = ReadInt16();
                LastWidth = ReadInt16();
                LastHeight = ReadInt16();
                CharactersPerRow = ReadInt16();
                RowCount = ReadInt16();
                CharacterWidth = ReadInt16();
                CharacterHeight = ReadInt16();
                ColumnWidth = ReadInt16();
                RowHeight = ReadInt16();

                // The next value appears to always be equal to CharacterHeight
                if (ReadInt16() != CharacterHeight)
                {
                    throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                }

                // The next two bytes appear to be 32 and 1
                if (!(reader.ReadByte() == 32 && reader.ReadByte() == 1))
                {
                    throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                }

                source.Position = entryTableOffset;

                Entries = new OrderedDictionary<char, FifEntry>(entryCount);

                for (var i = 0; i < entryCount; i++)
                {
                    var left = ReadInt32();
                    var right = ReadInt32();
                    var spacing = ReadInt32();
                    var character = ReadChar();
                    var index = ReadInt16();

                    // The final 2 bytes are the index of the character within the entry table
                    // If it's not equal to i, throw an exception.
                    if (index != i)
                    {
                        throw new FileFormatException(string.Format(Resources.InvalidFifFile, path));
                    }

                    Entries.Add(character, new FifEntry
                    {
                        Left = left,
                        Right = right,
                        Spacing = spacing,
                    });
                }
            }
        }

        /// <summary>
        /// Saves this <see cref="FifFile"/> to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        public void Save(string path)
        {
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                writer.Write(new byte[] { (byte)'F', (byte)'O', (byte)'N', (byte)'T', (byte)'D', (byte)'A', (byte)'T', (byte)'F' });

                Action<char> WriteChar;
                Action<short> WriteInt16;
                Action<int> WriteInt32;

                if (IsBigEndian)
                {
                    WriteChar = value => writer.Write((char)BinaryPrimitives.ReverseEndianness(value));
                    WriteInt16 = value => writer.Write(BinaryPrimitives.ReverseEndianness(value));
                    WriteInt32 = value => writer.Write(BinaryPrimitives.ReverseEndianness(value));

                    writer.Write(1);
                }
                else
                {
                    WriteChar = writer.Write;
                    WriteInt16 = writer.Write;
                    WriteInt32 = writer.Write;

                    writer.Write(0);
                }

                WriteInt16(101);
                WriteInt16(56);
                WriteInt32(Entries.Count);
                WriteInt32(CharactersPerTexture);
                WriteInt16(TextureCount);
                WriteInt16(Width);
                WriteInt16(Height);
                WriteInt16(LastWidth);
                WriteInt16(LastHeight);
                WriteInt16(CharactersPerRow);
                WriteInt16(RowCount);
                WriteInt16(CharacterWidth);
                WriteInt16(CharacterHeight);
                WriteInt16(ColumnWidth);
                WriteInt16(RowHeight);
                WriteInt16(CharacterHeight);
                writer.Write(288L); // Cheat and write this value out as a long

                short index = 0;
                foreach (var entry in Entries)
                {
                    WriteInt32(entry.Value.Left);
                    WriteInt32(entry.Value.Right);
                    WriteInt32(entry.Value.Spacing);
                    WriteChar(entry.Key);
                    WriteInt16(index);

                    index++;
                }
            }
        }
    }
}
