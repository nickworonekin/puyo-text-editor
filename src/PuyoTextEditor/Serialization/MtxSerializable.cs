using PuyoTextEditor.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PuyoTextEditor.Serialization
{
    [Serializable]
    [XmlRoot("mtx")]
    public class MtxSerializable
    {
        internal MtxSerializable()
        {
        }

        public MtxSerializable(MtxFile mtxFile)
        {
            Has64BitOffsets = mtxFile.Has64BitOffsets;

            Sheets = mtxFile.Entries.Select(x => new SheetEntry
            {
                Texts = x.Select(x2 => new XElement("text", x2.Nodes()))
                    .ToList(),
            }).ToList();
        }

        [XmlAttribute("has64BitOffsets")]
        public bool Has64BitOffsets { get; set; }

        public bool ShouldSerializeHas64BitOffsets() => Has64BitOffsets;

        [XmlElement("sheet")]
        public List<SheetEntry> Sheets { get; set; } = new List<SheetEntry>();
    }
}
