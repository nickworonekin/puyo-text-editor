using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PuyoTextEditor.Text
{
    public class CharacterMapMtxEncoding : MtxEncoding
    {
        private Dictionary<ushort, char> characterMap;

        public CharacterMapMtxEncoding(Dictionary<ushort, char> characterMap)
        {
            this.characterMap = characterMap;
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the specified string.
        /// </summary>
        /// <param name="s">The string containing the set of characters to encode</param>
        /// <returns>The number of bytes produced by encoding the specified characters.</returns>
        public override int GetByteCount(string s) => Encoding.Unicode.GetByteCount(Unescape(s)) + 2;

        /// <summary>
        /// Reads an encoded string from an MTX file.
        /// </summary>
        /// <param name="reader">An instance of <see cref="BinaryReader"/>.</param>
        /// <returns>The string.</returns>
        public override string Read(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            ushort @char;
            while ((@char = reader.ReadUInt16()) != 0xffff)
            {
                switch (@char)
                {
                    case 0xf800:
                        stringBuilder.Append($"{{color:{reader.ReadUInt16()}}}");
                        break;
                    case 0xf801:
                        stringBuilder.Append("{/color}");
                        break;
                    case 0xf812:
                        stringBuilder.Append("{clear}");
                        break;
                    case 0xf813:
                        stringBuilder.Append("{arrow}");
                        break;
                    case 0xf881:
                        stringBuilder.Append($"{{wait:{reader.ReadUInt16()}}}");
                        break;
                    case 0xfffd:
                        stringBuilder.Append("\n");
                        break;
                    default:
                        if (characterMap.ContainsKey(@char))
                        {
                            stringBuilder.Append(characterMap[@char]);
                        }
                        else
                        {
                            stringBuilder.Append(@"\u" + @char.ToString("x4"));
                        }
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        public override void Write(BinaryWriter writer, string s)
        {
            foreach (var @char in Unescape(s))
            {
                if (characterMap.ContainsValue(@char))
                {
                    writer.Write(characterMap.First(x => x.Value == @char).Key);
                }
                else
                {
                    writer.Write(@char);
                }
            }

            writer.Write('\uffff');
        }

        private static string Unescape(string s)
        {
            var patterns = new Dictionary<string, MatchEvaluator>
            {
                [@"\{color:(\d+)\}"] = match => "\uf800" + ((char)ushort.Parse(match.Groups[1].Value)),
                [@"\{/color\}"] = match => "\uf801",
                [@"\{clear\}"] = match => "\uf812",
                [@"\{arrow\}"] = match => "\uf813",
                [@"\{wait:(\d+)\}"] = match => "\uf881" + ((char)ushort.Parse(match.Groups[1].Value)),
                [@"\n"] = match => "\ufffd",
                [@"\\u(?<Value>[a-fA-F0-9]{4})"] = match => ((char)ushort.Parse(match.Groups["Value"].Value, NumberStyles.HexNumber)).ToString(),
            };

            return patterns.Aggregate(s, (current, replacement) => Regex.Replace(current, replacement.Key, replacement.Value));
        }
    }
}
