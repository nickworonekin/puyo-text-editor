using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PuyoTextEditor.FileFormats
{
    public class FntJsonFile
    {
        public int CharacterWidth { get; set; }

        public int CharacterHeight { get; set; }

        public SortedDictionary<char, FntJsonEntry> Entries { get; set; }

        public static FntJsonFile Read(string path)
        {
            string text;

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(source))
            {
                text = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<FntJsonFile>(text);
        }

        public void Write(string path)
        {
            var jsonToWrite = JsonConvert.SerializeObject(this);

            using (var textReader = new StringReader(jsonToWrite))
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var textWriter = new StreamWriter(destination, Encoding.UTF8))
            using (var jsonTextReader = new JsonTextReader(textReader))
            using (var jsonTextWriter = new JsonTextWriter(textWriter))
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = 4;
                jsonTextWriter.WriteToken(jsonTextReader);
            }
        }
    }
}
