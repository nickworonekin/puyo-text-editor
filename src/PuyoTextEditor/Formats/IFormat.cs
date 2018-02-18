namespace PuyoTextEditor.Formats
{
    public interface IFormat
    {
        /// <summary>
        /// Saves to the specified path.
        /// </summary>
        /// <param name="path">A string that contains the name of the path.</param>
        void Save(string path);
    }
}
