using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    public class SheetEntry
    {
        [XmlIgnore]
        public List<XElement> Texts
        {
            get => TextsSerialized.Select(x => TrimText(x)).ToList();
            set => TextsSerialized = value.Select(x => FormatText(x)).ToList();
        }

        [XmlAnyElement("text")]
        public List<XElement> TextsSerialized { get; set; } = new List<XElement>();

        public static XElement TrimText(XElement element)
        {
            // If the element has no content, then return it as-is.
            if (element.IsEmpty)
            {
                return element;
            }

            // Get the inner xml from the element.
            string innerXml;
            using (var reader = element.CreateReader())
            {
                reader.MoveToContent();
                innerXml = reader.ReadInnerXml();
            }

            // Split the string up, then determine what lines we want to take.
            var lines = innerXml.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var start = 0;
            var end = lines.Length;
            var shouldTrim = false;

            if (string.IsNullOrWhiteSpace(lines.First()))
            {
                shouldTrim = true;
                start++;

                if (string.IsNullOrWhiteSpace(lines.Last()))
                {
                    end--;
                }
            }

            // If we don't want to do any trimming, the just normalize the line endings
            if (!shouldTrim)
            {
                foreach (var tNode in element.Nodes().OfType<XText>())
                {
                    tNode.Value = Regex.Replace(tNode.Value, "\r\n|\r", "\n");
                }

                return element;
            }

            var linesToTake = lines.Skip(start).Take(end - start);

            // Get how much indent we want to remove
            var trimCount = int.MaxValue;
            foreach (var line in linesToTake)
            {
                if (line.Length < trimCount)
                {
                    trimCount = line.Length;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                for (var j = 0; j < trimCount && j < line.Length; j++)
                {
                    if (!char.IsWhiteSpace(line[j]))
                    {
                        trimCount = j;
                        break;
                    }
                }
            }

            // Create a new element with the trimmed content, then return it.
            var newInnerXml = string.Join('\n', linesToTake.Select(x => x[trimCount..]));
            var newElement = XElement.Parse($"<{element.Name}>{newInnerXml}</{element.Name}>", LoadOptions.PreserveWhitespace);
            newElement.Add(element.Attributes());

            return newElement;
        }

        private static XElement FormatText(XElement element)
        {
            foreach (var node in element.Nodes().OfType<XText>())
            {
                node.Value = node.Value.Replace("\n", "\n      ");
            }

            if (element.FirstNode is not null && element.LastNode is not null)
            {
                element.FirstNode.AddBeforeSelf("\n      ");
                element.LastNode.AddAfterSelf("\n    ");
            }

            return element;
        }
    }
}
