using PuyoTextEditor.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    [Serializable]
    [XmlRoot("fnt")]
    public class FntSerializable
    {
        internal FntSerializable()
        {
        }

        internal FntSerializable(FntFile fntFile)
        {
            Width = fntFile.Width;
            Height = fntFile.Height;
            Characters = fntFile.Entries.Select(x => new Char
            {
                Character = x.Key,
                Width = x.Value.Width,
            })
                .ToList();
        }

        [XmlElement("width")]
        public int Width { get; set; }

        [XmlElement("height")]
        public int Height { get; set; }

        [XmlElement("char")]
        public List<Char> Characters { get; set; } = new List<Char>();

        public class Char
        {
            [XmlAttribute("width")]
            public int Width { get; set; }

            [XmlIgnore]
            public char Character { get; set; }

            [XmlText]
            public string CharacterSerialized
            {
                get => Character.ToString();
                set => Character = value.Single();
            }
        }
    }
}
