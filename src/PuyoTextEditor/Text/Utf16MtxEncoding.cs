using PuyoTextEditor.Properties;
using PuyoTextEditor.Xml;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Text
{
    public class Utf16MtxEncoding : MtxEncoding
    {
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

            char c;
            while ((c = reader.ReadChar()) != '\uf8ff')
            {
                switch (c)
                {
                    case '\uf800':
                        builder.Push(new XElement("color", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case '\uf801':
                        builder.Pop();
                        break;
                    case '\uf812':
                        builder.Add(new XElement("clear"));
                        break;
                    case '\uf813':
                        builder.Add(new XElement("arrow"));
                        break;
                    case '\uf880':
                        builder.Add(new XElement("speed", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case '\uf881':
                        builder.Add(new XElement("wait", new XAttribute("value", reader.ReadUInt16())));
                        break;
                    case '\uf8fd':
                        builder.Add('\n');
                        break;
                    case '\uffff':
                        throw new InvalidDataException(Resources.FpdOrFntRequired);
                    default:
                        builder.Add(c);
                        break;
                }
            }

            return builder.ToXElement();
        }

        /// <inheritdoc/>
        public override void Write(BinaryWriter writer, XElement element)
        {
            WriteElement(writer, element);
            writer.Write('\uf8ff');
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
                            writer.Write('\uf800');
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));
                            WriteElement(writer, eNode);
                            writer.Write('\uf801');
                            break;
                        case "clear":
                            writer.Write('\uf812');
                            break;
                        case "arrow":
                            writer.Write('\uf813');
                            break;
                        case "speed":
                            writer.Write('\uf880');
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));
                            break;
                        case "wait":
                            writer.Write('\uf881');
                            writer.Write(ushort.Parse(eNode.AttributeOrThrow("value").Value));
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
                            writer.Write('\uf8fd');
                        }
                        else
                        {
                            writer.Write(c);
                        }
                    }
                }
            }
        }
    }
}
