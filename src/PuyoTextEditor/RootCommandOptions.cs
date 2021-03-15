using System.Collections.Generic;
using System.IO;

namespace PuyoTextEditor
{
    class RootCommandOptions
    {
        public string? Format { get; set; }

        public List<FileInfo> Files { get; set; } = new List<FileInfo>();

        public FileInfo? Fpd { get; set; }

        public FileInfo? Fnt { get; set; }

        public FileInfo? Output { get; set; }
    }
}
