using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PuyoTextEditor.Text
{
    public abstract class MtxEncoding
    {
        /// <summary>
        /// When overridden in a derived class, calculates the number of bytes produced by encoding the characters in the specified string.
        /// </summary>
        /// <param name="str">The string containing the set of characters to encode</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        public abstract int GetByteCount(string str);

        public abstract string Read(BinaryReader reader);

        public abstract void Write(BinaryWriter writer, string str);
    }
}
