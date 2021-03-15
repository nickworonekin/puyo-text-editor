namespace PuyoTextEditor.Formats
{
    public class CnvrsTextLayoutEntry
    {
        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public CnvrsTextTextAlignment TextAlignment { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        public CnvrsTextVerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Gets or sets whether word wrapping should be enabled.
        /// </summary>
        public bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets how text should be resized when it exceeds the bounds of its container.
        /// </summary>
        public CnvrsTextFit Fit { get; set; }
    }
}
