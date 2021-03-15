using System.Collections;

namespace PuyoTextEditor.Formats
{
    public class FntEntry
    {
        /// <summary>
        /// Gets or sets the width of this character
        /// </summary>
        public short Width { get; set; }

        public BitArray? Image { get; set; }
    }
}
