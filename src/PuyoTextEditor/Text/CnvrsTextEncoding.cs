using PuyoTextEditor.Properties;
using PuyoTextEditor.Xml;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Text
{
    public class CnvrsTextEncoding : IEncoding
    {
        /// <inheritdoc/>
        public int GetByteCount(XElement element)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.Unicode);

            Write(writer, element);

            return (int)stream.Length;
        }

        /// <param name="count">The number of characters to read.</param>
        /// <inheritdoc/>
        public XElement Read(BinaryReader reader, int? count = null)
        {
            // The count parameter is required.
            if (!count.HasValue)
            {
                throw new ArgumentNullException(nameof(count));
            }

            var builder = new TextEntryBuilder(new XElement("text"));

            var chars = reader.ReadChars(count.Value); // We're under the assumption that the reader encoding is Unicode.
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                switch (c & 0xF00F)
                {
                    case 0xE000: // Color
                        if (c == 0xE010) // Color end tag
                        {
                            builder.Pop();
                        }
                        else // Color start tag
                        {
                            i++;

                            var colorNameLength = GetNameLength(c) - 2;
                            var colorArgb = (uint)(chars[i] << 16 | (chars[i + 1]));
                            i += 2;
                            var colorName = new string(chars, i, colorNameLength);

                            builder.Push(new XElement("color",
                                new XAttribute("name", colorName),
                                new XAttribute("value", colorArgb.ToString("X8"))));

                            i += colorNameLength;
                        }

                        break;

                    case 0xE001: // Var(iable)
                        i++;

                        var varNameLength = GetNameLength(c);
                        var varName = new string(chars, i, varNameLength);

                        builder.Add(new XElement("var", new XAttribute("name", varName)));

                        i += varNameLength;

                        break;

                    case 0xE005: // Image
                        i++;

                        var imageNameLength = GetNameLength(c);
                        var imageName = new string(chars, i, imageNameLength);

                        builder.Add(new XElement("image", new XAttribute("name", imageName)));

                        i += imageNameLength;

                        break;

                    default: // All other characters (including new lines)
                        builder.Add(c);
                        break;
                }
            }

            return builder.ToXElement();
        }

        /// <inheritdoc/>
        public void Write(BinaryWriter writer, XElement element)
        {
            WriteElement(writer, element);
        }

        private void WriteElement(BinaryWriter writer, XElement element)
        {
            foreach (var node in element.Nodes())
            {
                if (node is XElement eNode)
                {
                    switch (eNode.Name.LocalName)
                    {
                        case "color":
                            var colorName = eNode.AttributeOrThrow("name").Value;
                            var colorArgb = uint.Parse(eNode.AttributeOrThrow("value").Value, NumberStyles.HexNumber);

                            writer.Write((ushort)(0xE000 | SetNameLength(colorName.Length + 2)));
                            writer.Write((ushort)(colorArgb >> 16));
                            writer.Write((ushort)(colorArgb & 0xFFFF));
                            writer.Write(colorName.ToCharArray());
                            writer.Write('\0');
                            WriteElement(writer, eNode);
                            writer.Write((ushort)0xE010);
                            break;
                        case "var":
                            var varName = eNode.AttributeOrThrow("name").Value;

                            writer.Write((ushort)(0xE001 | SetNameLength(varName.Length)));
                            writer.Write(varName.ToCharArray());
                            writer.Write('\0');
                            break;
                        case "image":
                            var imageName = eNode.AttributeOrThrow("name").Value;

                            writer.Write((ushort)(0xE005 | SetNameLength(imageName.Length)));
                            writer.Write(imageName.ToCharArray());
                            writer.Write('\0');
                            break;
                        default:
                            throw new InvalidDataException(string.Format(Resources.InvalidElement, eNode.Name.LocalName));
                    }
                }
                else if (node is XText tNode)
                {
                    writer.Write(tNode.Value.ToCharArray());
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort GetNameLength(ushort value) => (ushort)((((value & 0x0FF0) >> 4) / 2) - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort SetNameLength(int value) => (ushort)((((value + 1) * 2) << 4) & 0x0FF0);
    }
}
