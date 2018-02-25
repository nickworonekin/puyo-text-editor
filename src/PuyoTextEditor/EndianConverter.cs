using System;
using System.Collections.Generic;
using System.Text;

namespace PuyoTextEditor
{
    public static class EndianConverter
    {
        /// <summary>
        /// Converts the endian of a char value.
        /// </summary>
        /// <param name="value">The character to convert.</param>
        /// <returns>A char value.</returns>
        public static char Convert(char value)
        {
            return (char)((value & 0xFF) << 8 | (value >> 8) & 0xFF);
        }

        /// <summary>
        /// Converts the endian of a short value.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A short value.</returns>
        public static short Convert(short value)
        {
            return (short)((value & 0xFF) << 8 | (value >> 8) & 0xFF);
        }

        /// <summary>
        /// Converts the endian of an int value.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An int value.</returns>
        public static int Convert(int value)
        {
            return ((value & 0xFF) << 24 | (value & 0xFF00) << 8 | (value >> 8) & 0xFF00 | (value >> 24) & 0xFF);
        }

        /*/// <summary>
        /// Converts the endian of a long value.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A long value.</returns>
        public static long Convert(long value)
        {
            return (Convert(unchecked((int)value)) & 0xFFFFFFFF) << 32 | Convert(unchecked((int)(value >> 32))) & 0xFFFFFFFF;
        }*/
    }
}
