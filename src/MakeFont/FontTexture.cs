using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeFont
{
    class FontTexture
    {
        private readonly SortedDictionary<char, CharBitmap> charImages;
        private readonly int maxWidth;
        private readonly int maxHeight;
        private readonly int charWidth;
        private readonly int charHeight;
        private readonly Font font;

        public FontTexture(int maxWidth, int maxHeight, int charWidth, int charHeight, Font font = null)
        {
            charImages = new SortedDictionary<char, CharBitmap>();
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.charWidth = charWidth;
            this.charHeight = charHeight;
            this.font = font;
        }

        public void AddChar(char value)
        {
            if (charImages.ContainsKey(value))
            {
                throw new Exception();
            }

            var bitmap = new Bitmap(charWidth, charHeight);
            using (var graphics = Graphics.FromImage(bitmap))
            {
            }

            var characterTexture = new CharBitmap
            {
                Bitmap = bitmap,
                Width = 0,
                Height = 0,
            };

            charImages.Add(value, characterTexture);
        }

        public void AddCharFromImage(char value, string path)
        {
            if (charImages.ContainsKey(value))
            {
                throw new Exception();
            }

            var bitmap = new Bitmap(path);
            if (bitmap.Width != charWidth && bitmap.Height != charHeight)
            {
                throw new Exception();
            }

            var characterTexture = new CharBitmap
            {
                Bitmap = bitmap,
                Width = 0,
                Height = 0,
            };

            charImages.Add(value, characterTexture);
        }
    }
}
