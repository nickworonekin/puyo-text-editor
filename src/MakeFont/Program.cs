using McMaster.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MakeFont
{
    [Command(Name = "MakeFont",
        Description = "Creates font files for MTX files using MTX-converted JSON files.")]
    class Program
    {
        [Argument(0, "files",
            Description = "A list of MTX-converted JSON files. If omitted, ")]
        public List<string> Files { get; set; }

        [Required]
        [Option("-f | --format",
            Description = "The type of font definition file to create (fpd, fnt, fif, fifb).")]
        public string Format { get; set; }

        [HelpOption("-? | -h | --help",
            Description = "Show help information.")]
        public bool ShowHelp { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                CommandLineApplication.Execute<Program>("-?");
            }
            else
            {
                CommandLineApplication.Execute<Program>(args);
            }
        }

        public void OnExecute()
        {
            foreach (var file in Files)
            {
                //var fifFile = new FifFile(file);
                //Json.Write(file + ".json", fifFile);
                //var strings = MtxJsonFile.Read(file);
                //Console.WriteLine(string.Join("", GetUniqueChars(strings)));
            }
        }

        private List<char> GetUniqueChars(List<List<string>> strings)
        {
            // Merge all the strings together into one long string
            var str = string.Join(string.Empty, strings.Select(x => string.Join(string.Empty, x)));

            // Convert any control characters to their actual character
            // Then remove escape characters and curly braces
            var patterns = new Dictionary<string, MatchEvaluator>
            {
                [@"\\u(?<Value>[a-fA-F0-9]{4})"] = match => ((char)ushort.Parse(match.Groups["Value"].Value, NumberStyles.HexNumber)).ToString(),
                [@"\{.*}"] = match => string.Empty,
                [@"\n"] = match => string.Empty,
                
            };

            str = patterns.Aggregate(str, (current, replacement) => Regex.Replace(current, replacement.Key, replacement.Value));

            return str
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}
