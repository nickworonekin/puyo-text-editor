using System.IO;

namespace PuyoTextEditor.IO
{
    static class BinaryReaderExtensions
    {
        /// <summary>
        /// Returns the next available 4-byte signed integer and does not advance the byte or character position.
        /// </summary>
        /// <param name="reader">An instance of <see cref="BinaryReader"/>.</param>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        public static int PeekInt32(this BinaryReader reader)
        {
            var value = reader.ReadInt32();
            reader.BaseStream.Position -= sizeof(int);

            return value;
        }

        /// <summary>
        /// Returns the next available 8-byte signed integer and does not advance the byte or character position.
        /// </summary>
        /// <param name="reader">An instance of <see cref="BinaryReader"/>.</param>
        /// <returns>A 8-byte signed integer read from the current stream.</returns>
        public static long PeekInt64(this BinaryReader reader)
        {
            var value = reader.ReadInt64();
            reader.BaseStream.Position -= sizeof(long);

            return value;
        }
    }
}
