using McMaster.Extensions.CommandLineUtils;
using PuyoTextEditor.FileFormats;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtxToJson
{
    [Command(Name = "MtxToJson",
        Description = "Converts MTX files to JSON and visa-versa.")]
    class Program
    {
        [Argument(0, "files",
            Description = "The files to convert.")]
        public List<string> Files { get; set; }

        [Option("-f | --format <format>",
            Description = "The output format (auto, mtx, json). Defaults to auto.")]
        [Values("auto", "mtx", "json")]
        public string Format { get; set; }

        [Required]
        [Option("-v | --version <version>",
            Description = "The MTX version (1, 2). Use 1 for 15th, 7, and 20th. Use 2 for Tetris and Chronicle.")]
        [Values("1", "2")]
        public string Version { get; set; }

        [Option("--fnt <fnt>",
            Description = "The FNT file to use. Required when version is 1 and fpd is not set.")]
        public string FntPath { get; set; }

        [Option("--fpd <fpd>",
            Description = "The FPD file to use. Required when version is 1 and fnt is not set.")]
        public string FpdPath { get; set; }

        [Option("--64bit",
            Description = "When converting to MTX and version is 2, specifies offsets are 64-bit integers.")]
        public bool Has64BitOffsets { get; set; }

        [Option("-o | --output <output>",
            Description = "The output filename. Defaults to the input filename with an appropiate extension.")]
        public string Output { get; set; }

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
            // Don't execute if we are showing help.
            if (ShowHelp)
            {
                return;
            }

            MtxEncoding encoding = null;
            if (Version == "1")
            {
                if (FntPath != null)
                {
                    if (!File.Exists(FntPath))
                    {
                        Error($"\"{FntPath}\" does not exist or cannot be read.");
                        return;
                    }

                    var fntFile = FntFile.Read(FntPath);
                    var characterMap = fntFile.Select((item, index) => new
                    {
                        Index = (ushort)index,
                        Item = item.Character,
                    })
                    .ToDictionary(x => x.Index, x => x.Item);

                    encoding = new CharacterMapMtxEncoding(characterMap);
                }
                else if (FpdPath != null)
                {
                    if (!File.Exists(FpdPath))
                    {
                        Error($"\"{FpdPath}\" does not exist or cannot be read.");
                        return;
                    }

                    var fpdFile = FpdFile.Read(FpdPath);
                    var characterMap = fpdFile.Select((item, index) => new
                    {
                        Index = (ushort)index,
                        Item = item.Character,
                    })
                    .ToDictionary(x => x.Index, x => x.Item);
                    encoding = new CharacterMapMtxEncoding(characterMap);
                }
                else
                {
                    Error("The fpd or fnt option must be set when version is 1.");
                    return;
                }
            }
            else if (Version == "2")
            {
                encoding = new Utf16MtxEncoding();
            }

            foreach (var file in Files)
            {
                if (!File.Exists(file))
                {
                    Error($"\"{file}\" does not exist or cannot be read. Skipping file...");
                    continue;
                }

                var format = Format ?? "auto";
                if (format == "auto")
                {
                    var extension = Path.GetExtension(file);

                    if (extension.Equals(".mtx", StringComparison.OrdinalIgnoreCase))
                    {
                        format = "json";
                    }
                    else if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                    {
                        format = "mtx";
                    }
                    else
                    {
                        Error($"Could not determine output format for \"{file}\". Skipping file...");
                        continue;
                    }
                }

                if (format == "json")
                {
                    var outputFilename = Output ?? Path.ChangeExtension(file, ".json");

                    try
                    {
                        var mtxFile = MtxFile.Read(file, encoding);
                        var mtxJsonFile = new MtxJsonFile(mtxFile);
                        mtxJsonFile.Write(outputFilename);
                    }
                    catch (Exception e)
                    {
                        Error(e.Message);
                    }

                }
                else if (format == "mtx")
                {
                    var outputFilename = Output ?? Path.ChangeExtension(file, ".mtx");

                    try
                    {
                        var mtxJsonFile = MtxJsonFile.Read(file);
                        var mtxFile = new MtxFile(mtxJsonFile, encoding, Has64BitOffsets);
                        mtxFile.Write(outputFilename);
                    }
                    catch (Exception e)
                    {
                        Error(e.Message);
                    }
                }
            }
        }

        private static void Error(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(value);
            Console.ResetColor();
        }
    }
}
