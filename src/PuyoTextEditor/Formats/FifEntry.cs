namespace PuyoTextEditor.Formats
{
    public class FifEntry
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the left edge of this character.
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the right edge of this character.
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// Gets or sets the spacing between this character and the following character.
        /// </summary>
        public int Spacing { get; set; }
    }
}
