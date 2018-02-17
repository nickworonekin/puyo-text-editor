using System;
using System.Collections.Generic;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    class MtxSection
    {
        public int Position { get; set; }

        public List<MtxString> Strings { get; set; }

        public int StringsCount { get; set; }
    }
}
