using System.Xml.Serialization;

namespace PuyoTextEditor.Formats
{
    /// <summary>
    /// Specifies how text should be resized when it exceeds the bounds of its container.
    /// </summary>
    public enum CnvrsTextFit : int
    {
        /// <summary>
        /// Text should not be resized.
        /// </summary>
        [XmlEnum("none")]
        None = 0,

        /// <summary>
        /// Text should be scaled down.
        /// </summary>
        [XmlEnum("scaleDown")]
        ScaleDown = 1,

        /// <summary>
        /// Text should be condensed.
        /// </summary>
        [XmlEnum("condense")]
        Condense = 2,
    }
}
