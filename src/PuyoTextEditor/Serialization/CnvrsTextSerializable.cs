using PuyoTextEditor.Formats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    [Serializable]
    [XmlRoot("cnvrsText")]
    public class CnvrsTextSerializable
    {
        internal CnvrsTextSerializable()
        {
        }

        public CnvrsTextSerializable(CnvrsTextFile cnvrsTextFile)
        {
            Sheets = cnvrsTextFile.Sheets.Select(x => new Sheet
            {
                Name = x.Key,
                Texts = x.Value.Select(x2 => CreateTextElement(x2.Key, x2.Value))
                    .ToList(),
            }).ToList();

            Fonts = cnvrsTextFile.Fonts.Select(x => new Font
            {
                Name = x.Key,
                Typeface = x.Value.Typeface,
                Size = x.Value.Size,
                LineSpacing = x.Value.LineSpacing,
                Unknown1 = x.Value.Unknown1,
                Color = x.Value.Color,
                Unknown2 = x.Value.Unknown2,
                Unknown3 = x.Value.Unknown3,
                Unknown4 = x.Value.Unknown4,
            }).ToList();

            Layouts = cnvrsTextFile.Layouts.Select(x => new Layout
            {
                Name = x.Key,
                TextAlignment = x.Value.TextAlignment,
                VerticalAlignment = x.Value.VerticalAlignment,
                WordWrap = x.Value.WordWrap,
                Fit = x.Value.Fit,
            }).ToList();
        }

        private XElement CreateTextElement(string name, CnvrsTextEntry entry)
        {
            var element = new XElement("text",
                new XAttribute("name", name),
                new XAttribute("id", entry.Id));
            if (entry.FontName is not null)
            {
                element.Add(new XAttribute("font", entry.FontName));
            }
            if (entry.LayoutName is not null)
            {
                element.Add(new XAttribute("layout", entry.LayoutName));
            }

            element.Add(entry.Text.Nodes());

            return element;
        }

        [XmlElement("sheet")]
        public List<Sheet> Sheets { get; set; } = new List<Sheet>();

        [XmlArray("fonts")]
        [XmlArrayItem("font")]
        public List<Font> Fonts { get; set; } = new List<Font>();

        public bool ShouldSerializeFonts() => Fonts.Any();

        [XmlArray("layouts")]
        [XmlArrayItem("layout")]
        public List<Layout> Layouts { get; set; } = new List<Layout>();

        public bool ShouldSerializeLayouts() => Layouts.Any();

        public class Sheet : SheetEntry
        {
            [XmlAttribute("name")]
            public string Name { get; set; } = default!;
        }

        public class Font
        {
            [XmlAttribute("name")]
            public string Name { get; set; } = default!;

            [XmlElement("typeface")]
            public string Typeface { get; set; } = default!;

            [XmlElement("size")]
            public float Size { get; set; }

            [XmlElement("lineSpacing")]
            public float? LineSpacing { get; set; }

            public bool ShouldSerializeLineSpacing() => LineSpacing.HasValue;

            [XmlElement("unknown1")]
            public uint? Unknown1 { get; set; }

            public bool ShouldSerializeUnknown1() => Unknown1.HasValue;

            [XmlIgnore]
            public uint? Color { get; set; }

            [XmlElement("color")]
            public string? ColorSerialized
            {
                get => Color?.ToString("X8");
                set => Color = value is not null
                    ? uint.Parse(value, NumberStyles.HexNumber)
                    : null;
            }

            public bool ShouldSerializeColorSerialized() => Color.HasValue;

            [XmlElement("unknown2")]
            public uint? Unknown2 { get; set; }

            public bool ShouldSerializeUnknown2() => Unknown2.HasValue;

            [XmlElement("unknown3")]
            public uint? Unknown3 { get; set; }

            public bool ShouldSerializeUnknown3() => Unknown3.HasValue;

            [XmlElement("unknown4")]
            public uint? Unknown4 { get; set; }

            public bool ShouldSerializeUnknown4() => Unknown4.HasValue;
        }

        public class Layout
        {
            [XmlAttribute("name")]
            public string Name { get; set; } = default!;

            [XmlElement("textAlignment")]
            public CnvrsTextTextAlignment TextAlignment { get; set; }

            [XmlElement("verticalAlignment")]
            public CnvrsTextVerticalAlignment VerticalAlignment { get; set; }

            [XmlElement("wordWrap")]
            public bool WordWrap { get; set; }

            [XmlElement("fit")]
            public CnvrsTextFit Fit { get; set; }
        }
    }
}
