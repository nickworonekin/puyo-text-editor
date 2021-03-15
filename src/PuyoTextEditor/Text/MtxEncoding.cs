using System.IO;
using System.Xml.Linq;

namespace PuyoTextEditor.Text
{
    public abstract class MtxEncoding : IEncoding
    {
        /// <inheritdoc/>
        public abstract int GetByteCount(XElement element);

        /// <param name="count">This parameter is not used.</param>
        /// <inheritdoc/>
        public abstract XElement Read(BinaryReader reader, int? count = null);

        /// <inheritdoc/>
        public abstract void Write(BinaryWriter writer, XElement element);
    }
}
