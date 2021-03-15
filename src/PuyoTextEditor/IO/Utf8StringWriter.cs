using System;
using System.IO;
using System.Text;

namespace PuyoTextEditor.IO
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> for writing information to a UTF-8 encoded string. The information is stored in an underlying <see cref="StringBuilder"/>.
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        private static readonly Lazy<Encoding> utf8NoBomEncoding = new Lazy<Encoding>(() => new UTF8Encoding(false));

        /// <inheritdoc/>
        public override Encoding Encoding => utf8NoBomEncoding.Value;
    }
}
