using PuyoTextEditor.IO;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    /// <summary>
    /// Methods for serializing and deserializing XML using UTF-8 encoding.
    /// </summary>
    internal static class Utf8XmlSerializer
    {
        public static string Serialize<T>(T source)
        {
            var writerSettings = new XmlWriterSettings
            {
                Indent = true,
            };

            return Serialize(source, writerSettings);
        }

        public static string Serialize<T>(T source, XmlWriterSettings writerSettings)
        {
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add(string.Empty, string.Empty);

            var serializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new Utf8StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, writerSettings))
            {
                serializer.Serialize(xmlWriter, source, serializerNamespaces);
                return stringWriter.ToString();
            }
        }

        public static T? Deserialize<T>(string source)
        {
            var readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = false,
            };

            return Deserialize<T>(source, readerSettings);
        }

        public static T? Deserialize<T>(string source, XmlReaderSettings readerSettings)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stringReader = new StringReader(source))
            using (var xmlReader = XmlReader.Create(stringReader, readerSettings))
            {
                return (T?)serializer.Deserialize(xmlReader);
            }
        }
    }
}
