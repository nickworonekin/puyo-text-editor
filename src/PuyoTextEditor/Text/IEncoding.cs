using System.IO;
using System.Xml.Linq;

namespace PuyoTextEditor.Text
{
    public interface IEncoding
    {
        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the nodes of the specified <see cref="XElement"/>.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> containing the set of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        int GetByteCount(XElement element);

        /// <summary>
        /// Reads a string from the specified <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">An instance of <see cref="BinaryReader"/>.</param>
        /// <param name="count">If required by the <see cref="IEncoding"/>, the number of characters to read.</param>
        /// <returns>The <see cref="XElement"/> representation of the string.</returns>
        XElement Read(BinaryReader reader, int? count = null);

        /// <summary>
        /// Writes the contents of a <see cref="XElement"/> to the specified <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">An instance of <see cref="BinaryWriter"/>.</param>
        /// <param name="element">The <see cref="XElement"/> representation of the string.</param>
        void Write(BinaryWriter writer, XElement element);
    }
}
