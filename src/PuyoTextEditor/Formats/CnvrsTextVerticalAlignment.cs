using System.Xml.Serialization;

namespace PuyoTextEditor.Formats
{
    /// <summary>
    /// Specifies the vertical alignment of the text.
    /// </summary>
    public enum CnvrsTextVerticalAlignment : int
    {
        /// <summary>
        /// Text is vertically aligned to the top.
        /// </summary>
        [XmlEnum("top")]
        Top = 0,

        /// <summary>
        /// Text is vertically aligned in the middle.
        /// </summary>
        [XmlEnum("middle")]
        Middle = 1,

        /// <summary>
        /// Text is vertically aligned to the bottom.
        /// </summary>
        [XmlEnum("bottom")]
        Bottom = 2,
    }
}
