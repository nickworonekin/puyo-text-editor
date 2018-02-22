using McMaster.Extensions.CommandLineUtils;
using MtxToJson.Resources;
using PuyoTextEditor.Formats;
using PuyoTextEditor.Serialization;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        [AllowedValues("auto", "mtx", "json")]
        public string Format { get; set; }

        [Option("-v | --version <version>",
            Description = "The MTX version (auto, 1, 2). Use 1 for 15th, 7, and 20th. Use 2 for Tetris and Chronicle. Defaults to auto.")]
        [AllowedValues("auto", "1", "2")]
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
            var version = Version ?? "auto";
            if (version == "auto")
            {
                if (FntPath != null || FpdPath != null)
                {
                    version = "1";
                }
                else
                {
                    version = "2";
                }
            }
            if (version == "1")
            {
                if (FntPath != null)
                {
                    if (!File.Exists(FntPath))
                    {
                        Error(string.Format(ErrorMessages.FileDoesNotExist, FntPath));
                        return;
                    }

                    var fntFile = new FntFile(FntPath);
                    var chars = fntFile.Entries
                        .Select(x => x.Key)
                        .ToList();

                    encoding = new CharacterMapMtxEncoding(chars);
                }
                else if (FpdPath != null)
                {
                    if (!File.Exists(FpdPath))
                    {
                        Error(string.Format(ErrorMessages.FileDoesNotExist, FpdPath));
                        return;
                    }

                    var fpdFile = new FpdFile(FpdPath);
                    var chars = fpdFile.Entries
                        .Select(x => x.Key)
                        .ToList();

                    encoding = new CharacterMapMtxEncoding(chars);
                }
                else
                {
                    Error(ErrorMessages.FntOrFpdOptionMustBeSet);
                    return;
                }
            }
            else if (version == "2")
            {
                encoding = new Utf16MtxEncoding();
            }

            foreach (var file in Files)
            {
                if (!File.Exists(file))
                {
                    Error(string.Format(ErrorMessages.FileDoesNotExist, file));
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
                        Error(string.Format(ErrorMessages.CouldNotDetermineOutputFormat, file));
                        continue;
                    }
                }

                if (format == "json")
                {
                    var outputFilename = Output ?? Path.ChangeExtension(file, ".json");

                    try
                    {
                        var mtxFile = new MtxFile(file, encoding);
                        Json.Write(outputFilename, mtxFile.Entries);
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
                        var strings = Json.Read<List<List<string>>>(file);
                        var mtxFile = new MtxFile(strings, encoding, Has64BitOffsets);
                        mtxFile.Save(outputFilename);
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
