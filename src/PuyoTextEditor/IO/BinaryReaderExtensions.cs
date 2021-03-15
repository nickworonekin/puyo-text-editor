using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        /// <summary>
        /// Invokes <paramref name="func"/> at the current position of the stream and does not advance the byte or character position.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        public static void Peek(this BinaryReader reader, Action<BinaryReader> func)
        {
            var origPosition = reader.BaseStream.Position;

            try
            {
                func(reader);
            }
            finally
            {
                reader.BaseStream.Position = origPosition;
            }
        }

        /// <summary>
        /// Invokes <paramref name="func"/> at the current position of the stream and does not advance the byte or character position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="func"></param>
        /// <returns>The value returned by <paramref name="func"/>.</returns>
        public static T Peek<T>(this BinaryReader reader, Func<BinaryReader, T> func)
        {
            var origPosition = reader.BaseStream.Position;

            T value;
            try
            {
                value = func(reader);
            }
            finally
            {
                reader.BaseStream.Position = origPosition;
            }

            return value;
        }

        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static void At(this BinaryReader reader, long position, Action<BinaryReader> func)
        {
            var origPosition = reader.BaseStream.Position;
            if (origPosition != position)
            {
                reader.BaseStream.Position = position;
            }

            try
            {
                func(reader);
            }
            finally
            {
                reader.BaseStream.Position = origPosition;
            }
        }

        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <returns>The value returned by <paramref name="func"/>.</returns>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static T At<T>(this BinaryReader reader, long position, Func<BinaryReader, T> func)
        {
            var origPosition = reader.BaseStream.Position;
            if (origPosition != position)
            {
                reader.BaseStream.Position = position;
            }

            T value;
            try
            {
                value = func(reader);
            }
            finally
            {
                reader.BaseStream.Position = origPosition;
            }

            return value;
        }

        /// <summary>
        /// Reads a null-terminated string from the current stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadNullTerminatedString(this BinaryReader reader) => ReadNullTerminatedString(reader, Encoding.UTF8);

        /// <summary>
        /// Reads a null-terminated string from the current stream with the specified encoding.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns></returns>
        public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding)
        {
            var bytes = new List<byte>();

            byte c;
            while ((c = reader.ReadByte()) != 0)
            {
                bytes.Add(c);
            }

            return encoding.GetString(bytes.ToArray());
        }
    }
}
