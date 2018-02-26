using Newtonsoft.Json;
using System.IO;

namespace MtxToJson.Serialization
{
    public static class JsonFileSerializer
    {
        public static T Deserialize<T>(string path)
        {
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(source))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static void Serialize(string path, object obj)
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