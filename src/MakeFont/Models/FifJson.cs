using PuyoTextEditor.Collections;
using PuyoTextEditor.Formats;

namespace MakeFont.Models
{
    public class FifJson
    {
        public short CharacterWidth { get; set; }

        public short CharacterHeight { get; set; }

        public OrderedDictionary<char, FifEntry> Entries { get; set; }
    }
}
