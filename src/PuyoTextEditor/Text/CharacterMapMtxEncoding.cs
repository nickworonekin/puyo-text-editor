using PuyoTextEditor.IO;
using PuyoTextEditor.Properties;
using PuyoTextEditor.Xml;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Text
{
    public class CharacterMapMtxEncoding : MtxEncoding
    {
        private readonly Dictionary<ushort, char> indexToCharDictionary;
        private readonly Dictionary<char, ushort> charToIndexDictionary;

        public CharacterMapMtxEncoding(ICollection<char> chars)
        {
            indexToCharDictionary = new Dictionary<ushort, char>(chars.Count);
            charToIndexDictionary = new Dictionary<char, ushort>(chars.Count);

            ushort index = 0;
            foreach (var item in chars)
            {
                indexToCharDictionary.Add(index, item);
                if (!charToIndexDictionary.ContainsKey(item))
                {
                    charToIndexDictionary.Add(item, index);
                }
                index++;
            }
        }

        /// <inheritdoc/>
        public override int GetByteCount(XElement element)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.Unicode);

            Write(writer, element);

            return (int)stream.Length;
        }

        /// <inheritdoc/>
        public override XElement Read(BinaryReader reader, int? count = null)
        {
            var builder = new TextEntryBuilder(new XElement("text"));

            ushort c;
            while ((c = reader.ReadUInt16()) != 0xffff)
            {
                switch (c)
                {
                    case 0xf800:
                        // If the color element is the current element, convert it to an empty element
                        // and move its nodes to its parent element.
                        if (builder.Current.Name.LocalName == "color")
                        {
                            builder.ReplaceWithAdd();
                        }

                        builder.Push(new XElement("color", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case 0xf801:
                        builder.Pop();
                        break;
                    case 0xf812:
                        builder.Add(new XElement("clear"));
                        break;
                    case 0xf813:
                        builder.Add(new XElement("arrow"));
                        break;
                    case 0xf880:
                        builder.Add(new XElement("speed", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case 0xf881:
                        builder.Add(new XElement("wait", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case 0xf884:
                        builder.Add(new XElement("tutorialCourse"));
                        break;
                    case 0xf885:
                        builder.Add(new XElement("tutorialLevel"));
                        break;
                    case 0xf886:
                        builder.Add(new XElement("tutorialQuestion"));
                        break;
                    case 0xfffd:
                        builder.Add('\n');
                        break;
                    case 0xfffe:
                        builder.Add(new XElement("r"));
                        break;
                    case 0xf8ff:
                        throw new InvalidDataException(Resources.FpdOrFntNotRequired);
                    default:
                        if (indexToCharDictionary.TryGetValue(c, out var value))
                        {
                            builder.Add(value);
                        }
                        else
                        {
                            throw new KeyNotFoundException(string.Format(Resources.IndexNotFoundInFontFile, c));
                        }
                        break;
                }
            }

            // If the color element is the current element, convert it to an empty element
            // and move its nodes to its parent element.
            if (builder.Current.Name.LocalName == "color")
            {
                builder.ReplaceWithAdd();
            }

            return builder.ToXElement();
        }

        /// <inheritdoc/>
        public override void Write(BinaryWriter writer, XElement element)
        {
            WriteElement(writer, element);
            writer.WriteUInt16(0xffff);
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
                            writer.WriteUInt16(0xf800);
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));

                            // Only write the content and end tag if it's not an empty element.
                            if (!eNode.IsEmpty)
                            {
                                WriteElement(writer, eNode);
                                writer.WriteUInt16(0xf801);
                            }
                            break;
                        case "clear":
                            writer.WriteUInt16(0xf812);
                            break;
                        case "arrow":
                            writer.WriteUInt16(0xf813);
                            break;
                        case "speed":
                            writer.WriteUInt16(0xf880);
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));
                            break;
                        case "wait":
                            writer.WriteUInt16(0xf881);
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));
                            break;
                        case "tutorialCourse":
                            writer.WriteUInt16(0xf884);
                            break;
                        case "tutorialLevel":
                            writer.WriteUInt16(0xf885);
                            break;
                        case "tutorialQuestion":
                            writer.WriteUInt16(0xf886);
                            break;
                        case "r":
                            writer.WriteUInt16(0xfffe);
                            break;
                        default:
                            throw new InvalidDataException(string.Format(Resources.InvalidElement, eNode.Name.LocalName));
                    }
                }
                else if (node is XText tNode)
                {
                    foreach (var c in tNode.Value)
                    {
                        if (c == '\n')
                        {
                            writer.WriteUInt16(0xfffd);
                        }
                        else
                        {
                            if (charToIndexDictionary.TryGetValue(c, out var value))
                            {
                                writer.Write(value);
                            }
                            else
                            {
                                throw new KeyNotFoundException(string.Format(Resources.CharacterNotFoundInFontFile, c, (ushort)c));
                            }
                        }
                    }
                }
            }
        }
    }
}
