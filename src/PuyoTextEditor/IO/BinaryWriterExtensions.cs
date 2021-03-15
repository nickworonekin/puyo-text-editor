using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PuyoTextEditor.IO
{
    public static class BinaryWriterExtensions
    {
        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static void At(this BinaryWriter writer, long position, Action<BinaryWriter> func)
        {
            var origPosition = writer.BaseStream.Position;
            if (origPosition != position)
            {
                writer.BaseStream.Position = position;
            }

            try
            {
                func(writer);
            }
            finally
            {
                writer.BaseStream.Position = origPosition;
            }
        }

        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static void At(this BinaryWriter writer, long position, Action<BinaryWriter, long> func)
        {
            var origPosition = writer.BaseStream.Position;
            if (origPosition != position)
            {
                writer.BaseStream.Position = position;
            }

            try
            {
                func(writer, origPosition);
            }
            finally
            {
                writer.BaseStream.Position = origPosition;
            }
        }

        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <returns>The value returned by <paramref name="func"/>.</returns>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static T At<T>(this BinaryWriter writer, long position, Func<BinaryWriter, T> func)
        {
            var origPosition = writer.BaseStream.Position;
            if (origPosition != position)
            {
                writer.BaseStream.Position = position;
            }

            T value;
            try
            {
                value = func(writer);
            }
            finally
            {
                writer.BaseStream.Position = origPosition;
            }

            return value;
        }

        /// <summary>
        /// Invokes <paramref name="func"/> with the position of the stream set to the given value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="position"></param>
        /// <param name="func"></param>
        /// <returns>The value returned by <paramref name="func"/>.</returns>
        /// <remarks>The position of the stream is set to the previous position after <paramref name="func"/> is invoked.</remarks>
        public static T At<T>(this BinaryWriter writer, long position, Func<BinaryWriter, long, T> func)
        {
            var origPosition = writer.BaseStream.Position;
            if (origPosition != position)
            {
                writer.BaseStream.Position = position;
            }

            T value;
            try
            {
                value = func(writer, origPosition);
            }
            finally
            {
                writer.BaseStream.Position = origPosition;
            }

            return value;
        }

        /// <inheritdoc cref="BinaryWriter.Write(byte)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(byte)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteByte(this BinaryWriter writer, byte value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(short)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(short)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16(this BinaryWriter writer, short value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(int)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(int)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32(this BinaryWriter writer, int value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(long)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(long)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64(this BinaryWriter writer, long value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(ushort)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(short)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16(this BinaryWriter writer, ushort value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(uint)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(int)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32(this BinaryWriter writer, uint value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(ulong)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(long)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64(this BinaryWriter writer, ulong value) => writer.Write(value);

        /// <inheritdoc cref="BinaryWriter.Write(float)"/>
        /// <remarks>This method is an alias for <see cref="BinaryWriter.Write(float)"/>.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloat(this BinaryWriter writer, float value) => writer.Write(value);

        /// <summary>
        /// Writes a string to this stream, and advances the current position of the stream by the number of bytes specified in <paramref name="count"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <remarks><paramref name="value"/> will be truncated if the number of bytes is greater than <paramref name="count"/>.</remarks>
        public static void WriteString(this BinaryWriter writer, string value, int count) => WriteString(writer, value, count, Encoding.UTF8);

        /// <summary>
        /// Writes a string to this stream using the specified encoding, and advances the current position of the stream by the number of bytes specified in <paramref name="count"/>.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <remarks><paramref name="value"/> will be truncated if the number of bytes is greater than <paramref name="count"/>.</remarks>
        public static void WriteString(this BinaryWriter writer, string value, int count, Encoding encoding)
        {
            var bytes = encoding.GetBytes(value);
            if (bytes.Length >= count)
            {
                writer.Write(bytes, 0, count);
            }
            else
            {
                writer.Write(bytes);
                writer.Write(new byte[count - bytes.Length]);
            }
        }

        /// <summary>
        /// Writes a null-terminated string to this stream, and advances the current position of the stream by the length of <paramref name="value"/> in bytes plus one.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to write.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        public static int WriteNullTerminatedString(this BinaryWriter writer, string value) => WriteNullTerminatedString(writer, value, Encoding.UTF8);

        /// <summary>
        /// Writes a null-terminated string to this stream using the specified encoding, and advances the current position of the stream by the length of <paramref name="value"/> in bytes plus one.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value">The value to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        public static int WriteNullTerminatedString(this BinaryWriter writer, string value, Encoding encoding)
        {
            var bytes = encoding.GetBytes(value);
            writer.Write(bytes);
            writer.Write((byte)0);

            return bytes.Length + 1;
        }

        /// <summary>
        /// Aligns the stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="alignment"></param>
        public static void Align(this BinaryWriter writer, int alignment)
        {
            // No need to align if the BinaryWriter is already aligned.
            if (writer.BaseStream.Position % alignment == 0)
            {
                return;
            }

            writer.Write(new byte[(alignment - (writer.BaseStream.Position % alignment))]);
        }
    }
}
