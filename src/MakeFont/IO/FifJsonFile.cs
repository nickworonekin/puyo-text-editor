using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeFont.IO
{
    public class FifJsonFile
    {
        public short Width { get; set; }

        public short Height { get; set; }

        public SortedDictionary<char, FifJsonEntry> Characters { get; set; }
    }
}
