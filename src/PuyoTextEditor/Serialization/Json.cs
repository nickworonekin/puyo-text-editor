using Newtonsoft.Json;
using System.IO;

namespace PuyoTextEditor.Serialization
{
    public static class Json
    {
        public static T Read<T>(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(source))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static void Write(string path, object obj)
        {
            using (var destination = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var textWriter = new StreamWriter(destination))
            using (var jsonTextWriter = new JsonTextWriter(textWriter))
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.Indentation = 4;

                var serializer = new JsonSerializer();
                serializer.Serialize(jsonTextWriter, obj);
            }
        }
    }
}
