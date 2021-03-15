namespace PuyoTextEditor.Formats
{
    public class CnvrsTextFontEntry
    {
        /// <summary>
        /// Gets or sets the typeface.
        /// </summary>
        public string Typeface { get; set; } = default!;

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// Gets or sets the line spacing.
        /// </summary>
        public float? LineSpacing { get; set; }

        public uint? Unknown1 { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <remarks>The color is stored in the format ARGB.</remarks>
        public uint? Color { get; set; }

        public uint? Unknown2 { get; set; }

        public uint? Unknown3 { get; set; }

        public uint? Unknown4 { get; set; }
    }
}
