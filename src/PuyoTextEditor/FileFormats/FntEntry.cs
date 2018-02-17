using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class FntEntry
    {
        public char Character { get; set; }

        public short Width { get; set; }

        public BitArray Image { get; set; }
    }
}
