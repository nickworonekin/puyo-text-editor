using PuyoTextEditor.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    [Serializable]
    [XmlRoot("fpd")]
    public class FpdSerializable
    {
        internal FpdSerializable()
        {
        }

        internal FpdSerializable(FpdFile fpdFile)
        {
            Characters = fpdFile.Entries.Select(x => new Char
            {
                Character = x.Key,
                Width = x.Value.Width,
            })
                .ToList();
        }

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
