using PuyoTextEditor.IO;
using PuyoTextEditor.Properties;
using PuyoTextEditor.Serialization;
using PuyoTextEditor.Text;
using PuyoTextEditor.Xml;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Formats
{
    public class CnvrsTextFile : IFormat
    {
        private readonly CnvrsTextEncoding encoding = new CnvrsTextEncoding();

        private static readonly Dictionary<string, byte> languageCodes = new Dictionary<string, byte>
        {
            ["de"] = 0,
            ["en"] = 1,
            ["en(Rough)"] = 2, // This is never used but is a valid language code.
            ["es"] = 3,
            ["fr"] = 4,
            ["it"] = 5,
            ["ja"] = 6,
            ["ko"] = 7,
            ["zh"] = 8,
            ["zhs"] = 9,
        };

        /// <summary>
        /// Gets the collection of sheets that are currently in this file.
        /// </summary>
        public Dictionary<string, CnvrsTextSheetEntry> Sheets { get; } = new Dictionary<string, CnvrsTextSheetEntry>();

        /// <summary>
        /// Gets the collection of fonts that are currently in this file.
        /// </summary>
        public Dictionary<string, CnvrsTextFontEntry> Fonts { get; } = new Dictionary<string, CnvrsTextFontEntry>();

        /// <summary>
        /// Gets the collection of layouts that are currently in this file.
        /// </summary>
        public Dictionary<string, CnvrsTextLayoutEntry> Layouts { get; } = new Dictionary<string, CnvrsTextLayoutEntry>();

        public CnvrsTextFile(string path)
        {

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(source, Encoding.Unicode))
            {
                var binaSignature = Encoding.UTF8.GetString(reader.ReadBytes(8));
                if (binaSignature != "BINA210L")
                {
                    throw new FileFormatException(string.Format(Resources.InvalidCnvrsTextFile, path));
                }

                // The bytes at 0x8 tells us its expected file size.
                var length = reader.Peek(x => x.ReadInt32());
                if (length != source.Length)
                {
                    throw new FileFormatException(string.Format(Resources.InvalidCnvrsTextFile, path));
                }

                // We're under the assumption that there will only be one sheet per file.
                source.Position = 0x10 + 0x31;
                var sheetId = reader.ReadByte();
                var textStringsCount = reader.ReadInt16();

                source.Position = 0x10 + 0x40;
                var sheetName = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());
                var sheet = new CnvrsTextSheetEntry(textStringsCount)
                {
                    Id = sheetId,
                };
                Sheets.Add(sheetName, sheet);

                for (var i = 0; i < textStringsCount; i++)
                {
                    source.Position = 0x10 + 0x50 + (i * 0x30);

                    reader.ReadUInt64(); //Skip entry name hash.
                    var entryNameOffset = reader.ReadInt64() + 64;
                    var secondaryEntryOffset = reader.ReadInt64() + 64;
                    var textOffset = reader.ReadInt64() + 64;
                    var textLength = (int)reader.ReadInt64();

                    var parametersOffset = reader.ReadInt64();

                    var entryName = reader.At(entryNameOffset, x => x.ReadNullTerminatedString());

                    var entryNameHash = ComputeHash(entryName);

                    var entryText = reader.At(textOffset, x => encoding.Read(x, textLength));

                    source.Position = secondaryEntryOffset;

                    reader.ReadInt64(); // Skip
                    var entryFontEntryOffset = reader.ReadInt64();
                    var entryLayoutEntryOffset = reader.ReadInt64();

                    string? entryFontName = null;
                    string? entryLayoutName = null;
                    
                    if (entryFontEntryOffset != 0)
                    {
                        entryFontName = ReadFont(reader, entryFontEntryOffset + 64);
                    }

                    if (entryLayoutEntryOffset != 0)
                    {
                        entryLayoutName = ReadLayout(reader, entryLayoutEntryOffset + 64);
                    }

                    List<CnvrsParameterEntry> parameters = new();

                    if (parametersOffset != 0)
                    {
                        parameters = ReadParameters(reader, parametersOffset);
                    }

                    sheet.Entries.Add(entryName, new CnvrsTextEntry
                    {
                        Text = entryText,
                        FontName = entryFontName,
                        LayoutName = entryLayoutName,
                        Parameters = parameters
                    });
                }
            }
        }

        internal CnvrsTextFile()
        {
        }

        public CnvrsTextFile(CnvrsTextSerializable cnvrsTextSerializable)
        {
            Sheets = cnvrsTextSerializable.Sheets.ToDictionary(
                k => k.Name,
                v => new CnvrsTextSheetEntry
                {
                    Id = v.Index,
                    Entries = v.Texts.ToDictionary(
                        k2 => k2.AttributeOrThrow("name").Value,
                        v2 => new CnvrsTextEntry
                        {
                            FontName = v2.Attribute("font")?.Value,
                            LayoutName = v2.Attribute("layout")?.Value,
                            Text = new XElement("text", v2.Element("text")?.Nodes()),
                            Parameters = v2.Element("parameters")?.Elements("parameter")
                                .Select(paramElement => new CnvrsParameterEntry
                                {
                                    Unknown = ulong.Parse(paramElement.AttributeOrThrow("unknown").Value),
                                    Key = paramElement.AttributeOrThrow("key").Value,
                                    Value = paramElement.AttributeOrThrow("value").Value,
                                })
                                .ToList() ?? new List<CnvrsParameterEntry>()
                                })
                });

            Fonts = cnvrsTextSerializable.Fonts.ToDictionary(
                k => k.Name,
                v => new CnvrsTextFontEntry
                {
                    Typeface = v.Typeface,
                    Size = v.Size,
                    LineSpacing = v.LineSpacing,
                    Unknown1 = v.Unknown1,
                    Color = v.Color,
                    Unknown2 = v.Unknown2,
                    Unknown3 = v.Unknown3,
                    Unknown4 = v.Unknown4,
                });

            Layouts = cnvrsTextSerializable.Layouts.ToDictionary(
                k => k.Name,
                v => new CnvrsTextLayoutEntry
                {
                    TextAlignment = v.TextAlignment,
                    VerticalAlignment = v.VerticalAlignment,
                    WordWrap = v.WordWrap,
                    Fit = v.Fit,
                });
        }

        /// <summary>
        /// Saves this <see cref="CnvrsTextFile"/> to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        public void Save(string path)
        {
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(destination, Encoding.Unicode))
            {
                // Create the lists and dictionaries to hold offset information
                var offsets = new List<long>();
                var nameOffsets = new Dictionary<string, long>();
                var sheetNodes = new Dictionary<string, SheetNode>();
                var fontNodes = new Dictionary<string, FontNode>();
                var layoutNodes = new Dictionary<string, LayoutNode>();

                // A helper method to reduce the amount of code we need to write
                // For this to work as expected, offsets need to be written in the order they appear within the file.
                void writeOffset(long value)
                {
                    offsets.Add(destination.Position);
                    writer.WriteInt64(value);
                }

                // BINA section
                writer.Write(Encoding.UTF8.GetBytes("BINA210L")); // Always write as little endian
                writer.WriteInt32(0); // File length (filled in later)
                writer.WriteInt32(1);

                // DATA section
                writer.Write(Encoding.UTF8.GetBytes("DATA"));
                writer.WriteInt32(0); // Length of this section (filled in later)
                writer.WriteInt32(0); // Offset of name entry table (filled in later)
                writer.WriteInt32(0); // Length of name entry table (filled in later)
                writer.WriteInt32(0); // Length of final offset table (filled in later)
                writer.WriteInt32(24);
                writer.Write(new byte[24]); // 24 null bytes

                // Sheet entry table
                foreach (var (name, sheet) in Sheets)
                {
                    if (sheet.Id is null)
                    {
                        if (!languageCodes.TryGetValue(name, out var sheetId))
                        {
                            throw new KeyNotFoundException(string.Format(Resources.InvalidSheetName, name));
                        }

                        sheet.Id = sheetId;
                    }

                    var sheetNode = new SheetNode
                    {
                        EntryPosition = destination.Position,
                    };
                    sheetNodes.Add(name, sheetNode);

                    writer.WriteByte(6);
                    writer.WriteByte(sheet.Id.Value);
                    writer.WriteInt16((short)sheet.Entries.Count);
                    writer.WriteInt32(0); // 4 null bytes
                    writeOffset(0); // Primary entry offset (filled in later)
                    writeOffset(0); // Sheet name offset (filled in later)
                    writer.WriteInt64(0); // 8 null bytes
                }

                // Primary entries (per sheet)
                foreach (var (sheetName, sheet) in Sheets)
                {
                    var sheetNode = sheetNodes[sheetName];
                    sheetNode.TextNodeStartPosition = destination.Position;

                    foreach (var (textName, text) in sheet.Entries)
                    {
                        var textNode = new TextNode
                        {
                            EntryPosition = destination.Position,
                        };
                        sheetNode.TextNodes.Add(textName, textNode);

                        writer.WriteUInt64(ComputeHash(textName));
                        writeOffset(0); // Entry name offset (filled in later)
                        writeOffset(0); // Secondary entry offset (filled in later)
                        writeOffset(0); // Text string offset (filled in later)
                        writer.WriteInt64(encoding.GetByteCount(text.Text) / 2); // Length of text string in characters
                        writeOffset(0); // Parameter list offset (filled in later)
                    }
                }

                // Text strings
                foreach (var (sheetName, sheet) in Sheets)
                {
                    var sheetNode = sheetNodes[sheetName];

                    foreach (var (textName, text) in sheet.Entries)
                    {
                        var textNode = sheetNode.TextNodes[textName];
                        textNode.TextPosition = destination.Position;

                        encoding.Write(writer, text.Text);
                        writer.Write('\0');

                        writer.Align(8);
                    }
                }

                // Secondary offsets
                foreach (var (sheetName, texts) in Sheets)
                {
                    var sheetNode = sheetNodes[sheetName];

                    foreach (var textName in texts.Entries.Keys)
                    {
                        var textNode = sheetNode.TextNodes[textName];
                        textNode.SecondaryEntryPosition = destination.Position;

                        writeOffset(0); // Entry name offset (filled in later)
                        writeOffset(0); // Font entry offset (filled in later)
                        writeOffset(0); // Layout entry offset (filled in later)
                        writer.WriteInt64(0); // 8 null bytes
                    }
                }

                // Font entries
                foreach (var (name, font) in Fonts)
                {
                    var entryPosition = destination.Position;
                    var fontNode = new FontNode
                    {
                        EntryPosition = entryPosition,
                    };
                    fontNodes.Add(name, fontNode);

                    writeOffset(0); // Entry name offset (filled in later)
                    writeOffset(0); // Typeface name offset (filled in later)
                    writeOffset(entryPosition + 0x78 - 64); // Font size offset (always points to entry + 0x78)

                    if (font.LineSpacing.HasValue)
                    {
                        writeOffset(entryPosition + 0x80 - 64); // Line spacing offset (always points to entry + 0x80)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    if (font.Unknown1.HasValue)
                    {
                        writeOffset(entryPosition + 0x88 - 64); // Unknown 1 offset (always points to entry + 0x88)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    if (font.Color.HasValue && !font.Unknown1.HasValue) // This is intentional (unknown1 and color point to the same offset)
                    {
                        writeOffset(entryPosition + 0x88 - 64); // Color offset (always points to entry + 0x88)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    if (font.Unknown2.HasValue)
                    {
                        writeOffset(entryPosition + 0x90 - 64); // Unknown 2 offset (always points to entry + 0x90)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    writer.WriteInt64(0); // Always null

                    if (font.Unknown3.HasValue)
                    {
                        writeOffset(entryPosition + 0xA0 - 64); // Unknown 3 offset (always points to entry + 0xA0)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    writer.WriteInt64(0); // Always null
                    writer.WriteInt64(0); // Always null
                    writer.WriteInt64(0); // Always null

                    if (font.Unknown4.HasValue)
                    {
                        writeOffset(entryPosition + 0x98 - 64); // Unknown 4 offset (always points to entry + 0x98)
                    }
                    else
                    {
                        writer.WriteInt64(0);
                    }

                    writer.WriteInt64(0); // Always null
                    writer.WriteInt64(0); // Always null

                    writer.WriteFloat(font.Size);
                    writer.WriteInt32(0);
                    writer.WriteFloat(font.LineSpacing ?? 0);
                    writer.WriteInt32(0);
                    if (font.Unknown1.HasValue)
                    {
                        writer.WriteUInt32(font.Unknown1.Value);
                    }
                    else if (font.Color.HasValue)
                    {
                        writer.WriteUInt32(font.Color.Value);
                    }
                    else
                    {
                        writer.WriteUInt32(0);
                    }
                    writer.WriteInt32(0);
                    writer.WriteUInt32(font.Unknown2 ?? 0);
                    writer.WriteInt32(0);
                    writer.WriteUInt32(font.Unknown4 ?? 0);
                    writer.WriteInt32(0);
                    writer.WriteUInt32(font.Unknown3 ?? 0);
                    writer.WriteInt32(0);
                }

                // Layout entries
                foreach (var (name, layout) in Layouts)
                {
                    var entryPosition = destination.Position;
                    var layoutNode = new LayoutNode
                    {
                        EntryPosition = entryPosition,
                    };
                    layoutNodes.Add(name, layoutNode);

                    writeOffset(0); // Entry name offset (filled in later)
                    writer.Write(new byte[24]); // Unknown (just null bytes?)

                    writeOffset(entryPosition + 0x60 - 64); // Text alignment
                    writeOffset(entryPosition + 0x68 - 64); // Vertical alignment
                    writeOffset(entryPosition + 0x70 - 64); // Word wrap
                    writeOffset(entryPosition + 0x78 - 64); // Fit
                    writer.Write(new byte[32]); // Unknown (just null bytes?)

                    writer.WriteInt32((int)layout.TextAlignment);
                    writer.WriteInt32(0);
                    writer.WriteInt32((int)layout.VerticalAlignment);
                    writer.WriteInt32(0);
                    writer.WriteInt32(layout.WordWrap ? 1 : 0);
                    writer.WriteInt32(0);
                    writer.WriteInt32((int)layout.Fit);
                    writer.WriteInt32(0); // May not be needed
                }

                //Parameter pointer list headers
                foreach (var (sheetName, sheet) in Sheets)
                {
                    var sheetNode = sheetNodes[sheetName];
                    foreach (var (textName, text) in sheet.Entries)
                    {
                        if (text.Parameters.Count > 0)
                        {
                            var textNode = sheetNode.TextNodes[textName];
                            textNode.ParameterNodeStartPosition = destination.Position;
                            writer.WriteInt64(text.Parameters.Count);
                            writeOffset(0); // Parameter list offset (filled in later)

                            // Parameter pointer list entries
                            textNode.ParameterListStartPosition = destination.Position;
                            foreach (var parameter in text.Parameters)
                            {
                                writeOffset(0); // Parameter data pointer
                            }

                            // Parameter data entries
                            for (int i = 0; i < text.Parameters.Count; i++)
                            {
                                var parameter = text.Parameters[i];
                                var parameterNode = new ParameterNode
                                {
                                    EntryPosition = destination.Position,
                                };
                                textNode.ParameterNodes.Add(i, parameterNode);

                                writeOffset(0); // Key string offset (filled in later)
                                writer.WriteUInt64(parameter.Unknown);
                                writeOffset(0); // Value string offset (filled in later)
                            }
                        }
                    }
                }

                // Name entries
                var nameEntryPosition = destination.Position;
                foreach (var (sheetName, sheet) in Sheets)
                {
                    if (!nameOffsets.ContainsKey(sheetName))
                    {
                        nameOffsets.Add(sheetName, destination.Position);
                        writer.WriteNullTerminatedString(sheetName);
                    }

                    foreach (var textName in sheet.Entries.Keys)
                    {
                        if (!nameOffsets.ContainsKey(textName))
                        {
                            nameOffsets.Add(textName, destination.Position);
                            writer.WriteNullTerminatedString(textName);
                        }
                    }

                    foreach (var textName in sheet.Entries.Keys)
                    {
                        var entry = sheet.Entries[textName];
                        foreach(var parameter in entry.Parameters)
                        {
                            if (!nameOffsets.ContainsKey(parameter.Key))
                            {
                                nameOffsets.Add(parameter.Key, destination.Position);
                                writer.WriteNullTerminatedString(parameter.Key);
                            }
                            if (!nameOffsets.ContainsKey(parameter.Value))
                            {
                                nameOffsets.Add(parameter.Value, destination.Position);
                                writer.WriteNullTerminatedString(parameter.Value);
                            }
                        }

                    }
                }
                foreach (var (name, font) in Fonts)
                {
                    if (!nameOffsets.ContainsKey(name))
                    {
                        nameOffsets.Add(name, destination.Position);
                        writer.WriteNullTerminatedString(name);
                    }

                    if (!nameOffsets.ContainsKey(font.Typeface))
                    {
                        nameOffsets.Add(font.Typeface, destination.Position);
                        writer.WriteNullTerminatedString(font.Typeface);
                    }
                }
                foreach (var name in Layouts.Keys)
                {
                    if (!nameOffsets.ContainsKey(name))
                    {
                        nameOffsets.Add(name, destination.Position);
                        writer.WriteNullTerminatedString(name);
                    }
                }

                writer.Align(4);

                // Write the offset table
                // This contains a list of all the offsets located within the DATA section
                // Offsets are stored as relative to the previous offset.
                var offsetTablePosition = destination.Position;
                var prevOffset = 64L;
                foreach (var offset in offsets)
                {
                    var d = (uint)(offset - prevOffset) >> 2;

                    if (d <= 0x3F)
                    {
                        writer.WriteByte((byte)(0x40 | d)); // Starts with "01"
                    }
                    else if (d <= 0x3FFF)
                    {
                        writer.WriteUInt16(BinaryPrimitives.ReverseEndianness((ushort)((0x80 << 8) | d))); // Starts with "10"
                    }
                    else
                    {
                        writer.WriteUInt32(BinaryPrimitives.ReverseEndianness((uint)((0xC0 << 24) | d))); // Starts with "11"
                    }

                    prevOffset = offset;
                }

                writer.Align(4);

                // Go back and fill in all of the missing offsets
                destination.Position = 0x8;
                writer.WriteUInt32((uint)destination.Length);

                destination.Position = 0x14;
                writer.WriteUInt32((uint)destination.Length - 16);
                writer.WriteUInt32((uint)nameEntryPosition - 64);
                writer.WriteUInt32((uint)(offsetTablePosition - nameEntryPosition));
                writer.WriteUInt32((uint)(destination.Length - offsetTablePosition));

                foreach (var (sheetName, sheetNode) in sheetNodes)
                {
                    destination.Position = sheetNode.EntryPosition + 0x8;
                    writer.WriteInt64(sheetNode.TextNodeStartPosition - 64);
                    writer.WriteInt64(nameOffsets[sheetName] - 64);

                    foreach (var (textName, textNode) in sheetNode.TextNodes)
                    {
                        var fontName = Sheets[sheetName].Entries[textName].FontName;
                        var layoutName = Sheets[sheetName].Entries[textName].LayoutName;

                        destination.Position = textNode.EntryPosition + 0x8;
                        writer.WriteInt64(nameOffsets[textName] - 64);
                        writer.WriteInt64(textNode.SecondaryEntryPosition - 64);
                        writer.WriteInt64(textNode.TextPosition - 64);

                        destination.Position = textNode.EntryPosition + 0x28;
                        
                        writer.WriteInt64(textNode.ParameterNodes.Any() ? textNode.ParameterNodeStartPosition - 64 : 0);

                        destination.Position = textNode.SecondaryEntryPosition;
                        writer.WriteInt64(nameOffsets[textName] - 64);
                        writer.WriteInt64(fontName is not null
                            ? fontNodes[fontName].EntryPosition - 64
                            : 0);
                        writer.WriteInt64(layoutName is not null
                            ? layoutNodes[layoutName].EntryPosition - 64
                            : 0);
                    }
                }

                foreach (var (name, node) in fontNodes)
                {
                    var typefaceName = Fonts[name].Typeface;

                    destination.Position = node.EntryPosition;
                    writer.WriteInt64(nameOffsets[name] - 64);
                    writer.WriteInt64(nameOffsets[typefaceName] - 64);
                }
                
                foreach (var (name, node) in layoutNodes)
                {
                    destination.Position = node.EntryPosition;
                    writer.WriteInt64(nameOffsets[name] - 64);
                }

                foreach (var (sheetName, sheetNode) in sheetNodes)
                {
                    foreach (var (textName, textNode) in sheetNode.TextNodes)
                    {
                        if (textNode.ParameterNodes.Count > 0)
                        {
                            var textEntry = Sheets[sheetName].Entries[textName];
                            destination.Position = textNode.ParameterNodeStartPosition + 0x8;
                            writer.WriteInt64(textNode.ParameterListStartPosition - 64);

                            destination.Position = textNode.ParameterListStartPosition;
                            foreach(var (paramIndex, paramNode) in textNode.ParameterNodes)
                            {
                                writer.WriteInt64(paramNode.EntryPosition - 64);
                            }
                            foreach (var (paramIndex, paramNode) in textNode.ParameterNodes)
                            {
                                destination.Position = paramNode.EntryPosition;
                                writer.WriteInt64(nameOffsets[textEntry.Parameters[paramIndex].Key] - 64);
                                destination.Position = paramNode.EntryPosition + 0x10;
                                writer.WriteInt64(nameOffsets[textEntry.Parameters[paramIndex].Value] - 64);
                            }
                        }
                    }
                }

                destination.Seek(0, SeekOrigin.End);
            }
        }

        /// <summary>
        /// Computes and returns the hash of the given string. By ik_01 on Hedgehog Engine Modding Server.
        /// </summary>
        /// <param name="name">String to hash</param>
        /// <returns>The hash of the given string</returns>
        static uint ComputeHash(string name)
        {
            uint hash = 0u;
            for (int i = 0; i < name.Length; i++)
            {
                hash = hash * 0x7f + name[i];
            }
            return hash;
        }

        /// <summary>
        /// Reads the parameters from the text entry. Based on the work by Ashrindy: 
        /// https://github.com/Ashrindy/AshDumpLib/blob/6de90cf710c95e148ba9c9becb0cf43ef4c76d66/AshDumpLib/HedgehogEngine/BINA/Converse/Text.cs#L122
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="position">The position of the parameter list header node</param>
        /// <returns>List of parameters for this text entry</returns>
        private List<CnvrsParameterEntry> ReadParameters(BinaryReader reader, long position)
        {
            List<CnvrsParameterEntry> parameters = new();
            reader.BaseStream.Position = position + 64;
            var paramCount = reader.ReadInt64();
            var paramListPtr = reader.ReadInt64() + 64;
            reader.BaseStream.Position = paramListPtr;

            //For every parameter
            for (int paramIdx = 0; paramIdx < paramCount; paramIdx++)
            {
                //We read the data pointer and jump to it
                var currParamPtr = reader.ReadInt64() + 64;
                var lastPosition = reader.BaseStream.Position;
                reader.BaseStream.Position = currParamPtr;

                string paramKey = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());
                ulong paramUnknown = reader.ReadUInt64();
                string paramValue = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());
                CnvrsParameterEntry param = new()
                {
                    Unknown = paramUnknown,
                    Value = paramValue,
                    Key = paramKey
                };
                parameters.Add(param);

                reader.BaseStream.Position = lastPosition;
            }

            return parameters;
        }

        private string ReadFont(BinaryReader reader, long position)
        {
            reader.BaseStream.Position = position;

            var entryName = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());

            // If this font has already been read, no need to read it twice.
            if (Fonts.ContainsKey(entryName))
            {
                return entryName;
            }

            var typeface = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());
            var size = ReadValueAtOffset(reader, x => x.ReadSingle());
            var lineSpacing = ReadValueAtOffset<float?>(reader, x => x.ReadSingle());
            var unknown1 = ReadValueAtOffset<uint?>(reader, x => x.ReadUInt32());
            var color = ReadValueAtOffset<uint?>(reader, x => x.ReadUInt32());
            var unknown2 = ReadValueAtOffset<uint?>(reader, x => x.ReadUInt32());
            reader.BaseStream.Position += 8;
            var unknown3 = ReadValueAtOffset<uint?>(reader, x => x.ReadUInt32());
            reader.BaseStream.Position += 24;
            var unknown4 = ReadValueAtOffset<uint?>(reader, x => x.ReadUInt32());

            var font = new CnvrsTextFontEntry
            {
                Typeface = typeface,
                Size = size,
                LineSpacing = lineSpacing,
                Unknown1 = unknown1,
                Color = color,
                Unknown2 = unknown2,
                Unknown3 = unknown3,
                Unknown4 = unknown4,
            };

            Fonts.Add(entryName, font);

            return entryName;
        }

        private string ReadLayout(BinaryReader reader, long position)
        {
            reader.BaseStream.Position = position;

            var entryName = ReadValueAtOffsetOrThrow(reader, x => x.ReadNullTerminatedString());

            // If this layout has already been read, no need to read it twice.
            if (Layouts.ContainsKey(entryName))
            {
                return entryName;
            }

            reader.BaseStream.Position += 24;

            var textAlignment = (CnvrsTextTextAlignment)ReadValueAtOffset(reader, x => x.ReadInt32());
            var verticalAlignment = (CnvrsTextVerticalAlignment)ReadValueAtOffset(reader, x => x.ReadInt32());
            var wordWrap = ReadValueAtOffset(reader, x => x.ReadInt32()) == 1;
            var fit = (CnvrsTextFit)ReadValueAtOffset(reader, x => x.ReadInt32());

            var layout = new CnvrsTextLayoutEntry
            {
                TextAlignment = textAlignment,
                VerticalAlignment = verticalAlignment,
                WordWrap = wordWrap,
                Fit = fit,
            };

            Layouts.Add(entryName, layout);

            return entryName;
        }

        /// <summary>
        /// Reads the value located at the offset referenced at the current position of the <paramref name="reader"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        /// <param name="defaultValue">The default value to return when there is no value.</param>
        /// <returns>
        /// The value located at the offset referenced at the current position of the <paramref name="reader"/>,
        /// or <paramref name="defaultValue"/> if there is no value.
        /// </returns>
        private static T? ReadValueAtOffset<T>(BinaryReader reader, Func<BinaryReader, T?> func, T? defaultValue = default)
        {
            var position = reader.ReadInt64();
            if (position == 0)
            {
                return defaultValue;
            }

            return reader.At(position + 64, func);
        }

        /// <summary>
        /// Reads the value located at the offset referenced at the current position of the <paramref name="reader"/>,
        /// or throws a <see cref="NullValueException"/> if there is no value at the offset referenced.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        /// <returns>
        /// The value located at the offset referenced at the current position of the <paramref name="reader"/>.
        /// </returns>
        /// <exception cref="NullValueException">Thrown when there is no value at the offset referenced.</exception>
        private static T ReadValueAtOffsetOrThrow<T>(BinaryReader reader, Func<BinaryReader, T> func)
        {
            var position = reader.ReadInt64();
            if (position == 0)
            {
                throw new NullValueException();
            }

            return reader.At(position + 64, func);
        }

        private class SheetNode
        {
            public long EntryPosition;
            public long TextNodeStartPosition;

            public Dictionary<string, TextNode> TextNodes { get; } = new Dictionary<string, TextNode>();
        }

        private class TextNode
        {
            public long EntryPosition;
            public long SecondaryEntryPosition;
            public long TextPosition;
            public long ParameterNodeStartPosition;
            public long ParameterListStartPosition;
            public Dictionary<int, ParameterNode> ParameterNodes { get; } = new Dictionary<int, ParameterNode>();
        }

        private class ParameterNode
        {
            public long EntryPosition;
        }


        private class FontNode
        {
            public long EntryPosition;
        }

        private class LayoutNode
        {
            public long EntryPosition;
        }
    }
}
