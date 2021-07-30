using PuyoTextEditor.Properties;
using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PuyoTextEditor.Xml
{
    internal class TextEntryBuilder
    {
        private readonly StringBuilder stringBuilder;

        private readonly XElement rootElement;
        private XElement currentElement;

        public XElement Current => currentElement;

        public TextEntryBuilder(XElement element)
        {
            stringBuilder = new StringBuilder();

            rootElement = element;
            currentElement = rootElement;
        }

        public void Push(XElement element)
        {
            Add(element);
            currentElement = element;
        }

        public XElement Pop()
        {
            WriteText();

            if (currentElement.Parent is null)
            {
                // This can occur in valid MTX files.
                // In this case, don't pop the current element.
                return currentElement;
            }

            var element = currentElement;
            currentElement = element.Parent;

            return element;
        }

        public void Add(XElement value)
        {
            WriteText();

            currentElement.Add(value);
        }

        public void Add(char value)
        {
            stringBuilder.Append(value);
        }

        public void Add(string value)
        {
            stringBuilder.Append(value);
        }

        public void ReplaceWithAdd()
        {
            var element = Pop();

            // If the popped element is the current element, then do nothing.
            // This can happen if the current element is the root element.
            if (element == currentElement)
            {
                return;
            }

            element.Remove();

            var nodes = element.Nodes().ToList();
            element.RemoveNodes();

            currentElement.Add(element, nodes);
        }

        public XElement ToXElement()
        {
            WriteText();

            if (currentElement != rootElement)
            {
                throw new InvalidOperationException(Resources.XmlTagMismatch);
            }

            return rootElement;
        }

        private void WriteText()
        {
            if (stringBuilder.Length > 0)
            {
                currentElement.Add(new XText(stringBuilder.ToString()));
                stringBuilder.Clear();
            }
        }
    }
}
