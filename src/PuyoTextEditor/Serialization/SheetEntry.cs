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

        [XmlAnyElement("textEntry")]
        public List<XElement> TextsSerialized { get; set; } = new List<XElement>();

        public static XElement TrimText(XElement textEntryElement)
        {
            // Find the text element within textEntry
            var textElement = textEntryElement.Element("text");
            if (textElement == null || textElement.IsEmpty)
            {
                return textEntryElement;
            }

            // Get the inner xml from the text element
            string innerXml;
            using (var reader = textElement.CreateReader())
            {
                reader.MoveToContent();
                innerXml = reader.ReadInnerXml();
            }

            // Split the string up, then determine what lines we want to take
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

            // If we don't want to do any trimming, then just normalize the line endings
            if (!shouldTrim)
            {
                foreach (var tNode in textElement.Nodes().OfType<XText>())
                {
                    tNode.Value = Regex.Replace(tNode.Value, "\r\n|\r", "\n");
                }
                return textEntryElement;
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

            // Create a new text element with the trimmed content
            var newInnerXml = string.Join('\n', linesToTake.Select(x => x[trimCount..]));
            var newTextElement = XElement.Parse($"<text>{newInnerXml}</text>", LoadOptions.PreserveWhitespace);
            newTextElement.Add(textElement.Attributes());

            // Replace the old text element with the new one in the textEntry
            textElement.ReplaceWith(newTextElement);

            return textEntryElement;
        }

        private static XElement FormatText(XElement textEntryElement)
        {
            // Find the text element within textEntry
            var textElement = textEntryElement.Element("text");
            if (textElement == null) return textEntryElement;

            // Apply formatting to the text element's text nodes
            foreach (var node in textElement.Nodes().OfType<XText>())
            {
                node.Value = node.Value.Replace("\n", "\n          "); // 10 spaces for content
            }

            // Add indentation before first and after last node if content exists
            if (textElement.FirstNode is not null && textElement.LastNode is not null)
            {
                textElement.FirstNode.AddBeforeSelf("\n          "); // 10 spaces before content
                textElement.LastNode.AddAfterSelf("\n      "); // 6 spaces for closing text tag
            }

            return textEntryElement;
        }
    }
}
