using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PuyoTextEditor.Text
{
    public class Utf16MtxEncoding : MtxEncoding
    {
        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the specified string.
        /// </summary>
        /// <param name="str">The string containing the set of characters to encode</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        public override int GetByteCount(string s) => Encoding.Unicode.GetByteCount(Unescape(s)) + 2;

        public override string Read(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            char c;
            while ((c = reader.ReadChar()) != '\uf8ff')
            {
                switch (c)
                {
                    case '\uf800':
                        stringBuilder.Append($"{{color:{reader.ReadUInt16()}}}");
                        break;
                    case '\uf801':
                        stringBuilder.Append("{/color}");
                        break;
                    case '\uf812':
                        stringBuilder.Append("{clear}");
                        break;
                    case '\uf813':
                        stringBuilder.Append("{arrow}");
                        break;
                    case '\uf880':
                        stringBuilder.Append($"{{speed:{reader.ReadUInt16()}}}");
                        break;
                    case '\uf881':
                        stringBuilder.Append($"{{wait:{reader.ReadUInt16()}}}");
                        break;
                    case '\uf8fd':
                        stringBuilder.Append("\n");
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        public override void Write(BinaryWriter writer, string s)
        {
            writer.Write(Encoding.Unicode.GetBytes(Unescape(s)));
            writer.Write('\uf8ff');
        }

        private static string Unescape(string s)
        {
            var patterns = new Dictionary<string, MatchEvaluator>
            {
                [@"\{color:(\d+)\}"] = match => "\uf800" + ((char)ushort.Parse(match.Groups[1].Value)),
                [@"\{/color\}"] = match => "\uf801",
                [@"\{clear\}"] = match => "\uf812",
                [@"\{arrow\}"] = match => "\uf813",
                [@"\{speed:(\d+)\}"] = match => "\uf880" + ((char)ushort.Parse(match.Groups[1].Value)),
                [@"\{wait:(\d+)\}"] = match => "\uf881" + ((char)ushort.Parse(match.Groups[1].Value)),
                [@"\n"] = Match => "\uf8fd",
            };

            return patterns.Aggregate(s, (current, replacement) => Regex.Replace(current, replacement.Key, replacement.Value));
        }
    }
}
