using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuyoTextEditor.FileFormats
{
    public class MtxJsonFile : List<List<string>>
    {
        public MtxJsonFile()
            : base()
        {
        }

        public MtxJsonFile(List<List<string>> collection)
            : base(collection)
        {
        }

        public MtxJsonFile(int capacity)
            : base(capacity)
        {
        }

        public static MtxJsonFile Read(string path)
        {
            string text;

            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(source))
            {
                text = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<MtxJsonFile>(text);
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
