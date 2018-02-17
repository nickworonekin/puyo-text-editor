using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class FntFile : List<FntEntry>
    {
        public FntFile()
            : base()
        { }

        public FntFile(IEnumerable<FntEntry> collection)
            : base(collection)
        { }

        public FntFile(int capacity)
            : base(capacity)
        { }

        public static FntFile Read(string path)
        {
            var fntFile = new FntFile();

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                // FNT files start with the magic code FNT(null)
                if (!(reader.ReadByte() == 'F' && reader.ReadByte() == 'N' && reader.ReadByte() == 'T' && reader.ReadByte() == 0))
                {
                    throw new IOException($"{path} is not a valid FNT file.");
                }

                var characterHeight = reader.ReadInt32();
                var characterWidth = reader.ReadInt32();
                var characterCount = reader.ReadInt32();

                // The FNT fomat is slightly different between the DS version and the Wii & PSP versions.
                // It's possible to differentiate between the two by comparing the amount of characters it contains to its file size.
                // We will also use this to determine if it's a valid FNT file.
                bool hasImages;
                if (48 + (characterCount * (4 + (characterWidth * characterHeight / 2))) == source.Length)
                {
                    // DS FNT files
                    hasImages = true;
                    source.Position += 32;
                }
                else if (16 + (characterCount * 4) == source.Length)
                {
                    // Wii & PSP FNT files
                    hasImages = false;
                }
                else
                {
                    throw new IOException($"{path} is not a valid FNT file.");
                }

                while (source.Position < source.Length)
                {
                    var entry = new FntEntry
                    {
                        Character = reader.ReadChar(),
                        Width = reader.ReadInt16(),
                    };
                    fntFile.Add(entry);

                    if (hasImages)
                    {
                        source.Position += characterWidth * characterHeight / 2;
                    }
                }
            }

            return fntFile;
        }

        public void Write(string path, int characterWidth, int characterHeight, bool hasImages = false)
        {
            if (hasImages)
            {
                // Make sure the character width is a multiple of 2
                if (characterWidth % 2 != 0)
                {
                    throw new Exception();
                }

                // Make sure the Image property in entries is filled in and is of the correct length
                if (!this.All(x => x.Image != null && x.Image.Length == characterWidth * characterHeight))
                {
                    throw new Exception();
                }
            }

            // Sort the entries
            var sortedEntries = this.OrderBy(x => x.Character);

            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                writer.Write(new byte[] { (byte)'F', (byte)'N', (byte)'T', 0 });
                writer.Write(characterHeight);
                writer.Write(characterWidth);
                writer.Write(Count);

                if (hasImages)
                {
                    writer.Write(new byte[] { 0xE0, 0x03, 0xFF, 0x7F, 0xC6, 0x18 });
                    for (var i = 0; i < 26; i++)
                    {
                        writer.Write((byte)0);
                    }
                }

                foreach (var entry in sortedEntries)
                {
                    writer.Write(entry.Character);
                    writer.Write(entry.Width);

                    if (hasImages)
                    {
                        for (var i = 0; i < entry.Image.Length; i += 2)
                        {
                            var value = (byte)(((entry.Image[i] ? 1 : 0) << 4) | ((entry.Image[i + 1] ? 1 : 0) & 0xf));
                            writer.Write(value);
                        }
                    }
                }
            }
        }
    }
}
