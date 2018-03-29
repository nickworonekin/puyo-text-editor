﻿using PuyoTextEditor.Collections;
using PuyoTextEditor.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuyoTextEditor.Formats
{
    public class FntFile : IFormat
    {
        public int Width { get; }

        public int Height { get; }

        public bool HasImages { get; }

        /// <summary>
        /// Gets the collection of entries that are currently in this file.
        /// </summary>
        public OrderedDictionary<char, FntEntry> Entries { get; }

        /// <summary>
        /// Gets the characters that are currently in this file.
        /// </summary>
        public List<char> Characters { get; }

        public FntFile(int width, int height, bool hasImages)
        {
            Entries = new OrderedDictionary<char, FntEntry>();
            Characters = new List<char>();
        }

        public FntFile(IDictionary<char, FntEntry> collection, int width, int height, bool hasImages)
        {
            Entries = new OrderedDictionary<char, FntEntry>(collection);
            Characters = new List<char>(collection.Keys);
        }

        public FntFile(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // FNT files start with the magic code FNT(null)
                if (!(reader.ReadByte() == 'F' && reader.ReadByte() == 'N' && reader.ReadByte() == 'T' && reader.ReadByte() == 0))
                {
                    throw new IOException(string.Format(ErrorMessages.InvalidFntFile, path));
                }

                Height = reader.ReadInt32();
                Width = reader.ReadInt32();
                var characterCount = reader.ReadInt32();
                Entries = new OrderedDictionary<char, FntEntry>(characterCount);
                

                // The FNT fomat is slightly different between the DS version and the Wii & PSP versions.
                // It's possible to differentiate between the two by comparing the amount of characters it contains to its file size.
                // We will also use this to determine if it's a valid FNT file.
                if (48 + (characterCount * (4 + (Width * Height / 2))) == source.Length)
                {
                    // DS FNT files
                    HasImages = true;
                    source.Position += 32;
                }
                else if (16 + (characterCount * 4) == source.Length)
                {
                    // Wii & PSP FNT files
                    HasImages = false;
                }
                else
                {
                    throw new IOException(string.Format(ErrorMessages.InvalidFntFile, path));
                }

                while (source.Position < source.Length)
                {
                    var c = reader.ReadChar();
                    var width = reader.ReadInt16();

                    if (!Entries.ContainsKey(c))
                    {
                        Entries.Add(c, new FntEntry
                        {
                            Width = width,
                        });
                    }
                    Characters.Add(c);

                    if (HasImages)
                    {
                        source.Position += Width * Height / 2;
                    }
                }
            }
        }

        /// <summary>
        /// Saves this <see cref="FpdFile"/> to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        public void Save(string path)
        {
            if (HasImages)
            {
                // Make sure the character width is a multiple of 2
                if (Width % 2 != 0)
                {
                    throw new Exception();
                }

                // Make sure the Image property in entries is filled in and is of the correct length
                if (!Entries.Values.All(x => x.Image != null && x.Image.Length == Width * Height))
                {
                    throw new Exception();
                }
            }

            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                writer.Write(new byte[] { (byte)'F', (byte)'N', (byte)'T', 0 });
                writer.Write(Height);
                writer.Write(Width);
                writer.Write(Entries.Count);

                if (HasImages)
                {
                    writer.Write(new byte[] { 0xE0, 0x03, 0xFF, 0x7F, 0xC6, 0x18 });
                    for (var i = 0; i < 26; i++)
                    {
                        writer.Write((byte)0);
                    }
                }

                foreach (var entry in Entries)
                {
                    writer.Write(entry.Key);
                    writer.Write(entry.Value.Width);

                    if (HasImages)
                    {
                        for (var i = 0; i < entry.Value.Image.Length; i += 2)
                        {
                            var value = (byte)(((entry.Value.Image[i] ? 1 : 0) << 4) | ((entry.Value.Image[i + 1] ? 1 : 0) & 0xf));
                            writer.Write(value);
                        }
                    }
                }
            }
        }
    }
}
