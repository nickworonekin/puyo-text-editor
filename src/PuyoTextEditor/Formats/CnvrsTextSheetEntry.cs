using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoTextEditor.Formats
{
    public class CnvrsTextSheetEntry
    {
        /// <summary>
        /// Gets or sets the 8-bit identifier associated with this sheet's name.
        /// </summary>
        /// <remarks>If <see langword="null" />, this will be set based on the sheet's name when <see cref="CnvrsTextFile.Save(string)"/> is invoked.</remarks>
        public byte? Id { get; set; }

        /// <summary>
        /// Gets the collection of text entries that are currently in this sheet.
        /// </summary>
        public Dictionary<string, CnvrsTextEntry> Entries { get; set; }

        public CnvrsTextSheetEntry()
        {
            Entries = new Dictionary<string, CnvrsTextEntry>();
        }

        public CnvrsTextSheetEntry(int entriesCapacity)
        {
            Entries = new Dictionary<string, CnvrsTextEntry>(entriesCapacity);
        }
    }
}
