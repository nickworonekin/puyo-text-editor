using PuyoTextEditor.Formats;
using PuyoTextEditor.Serialization;
using PuyoTextEditor.Text;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace PuyoTextEditor
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Modify text in various Puyo Puyo games.")
            {
                new Option<string>(new string[] { "-f", "--format" }, "Set the text format. If not set, it will be determined by the file extension."),
                new Option<FileInfo>("--fpd", "Set the FPD font file when converting to and from MTX files.")
                    .ExistingOnly(),
                new Option<FileInfo>("--fnt", "Set the FNT font file when converting to and from MTX files.")
                    .ExistingOnly(),
                new Option<FileInfo>(new string[] { "-o", "--output"}, "Set the output filename. If not set, defaults to the input filename with an appropiate extension.")
                    .LegalFilePathsOnly(),
                new Argument<List<FileInfo>>("files", "The files to convert.")
                {
                    Arity = ArgumentArity.OneOrMore,
                }
                    .ExistingOnly(),
            };
            rootCommand.Handler = CommandHandler.Create<RootCommandOptions>(Convert);

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseExceptionHandler(ExceptionHandler)
                .Build();

            return await parser.InvokeAsync(args);
        }

        static void ExceptionHandler(Exception e, InvocationContext context)
        {
            // If the exception is a TargetInvocationException, use the underlying exception.
            if (e is TargetInvocationException
                && e.InnerException is not null)
            {
                e = e.InnerException;
            }

            // If the exception is an InvalidOperationException due to an XmlException,
            // use the XmlException as its error message is more descriptive.
            if (e is InvalidOperationException
                && e.InnerException is XmlException)
            {
                e = e.InnerException;
            }

            Console.ForegroundColor = ConsoleColor.Red;
#if DEBUG
            Console.Error.WriteLine(e);
#else
            Console.Error.WriteLine(e.Message);
#endif
            Console.ResetColor();

            context.ExitCode = e.HResult;
        }

        static void Convert(RootCommandOptions options)
        {
            foreach (var file in options.Files)
            {
                var filePath = file.ToString(); // Returns the original path supplied to FileInfo.

                // Convert from XML
                if (filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    // XML to MTX
                    if (string.Equals(options.Format, "mtx", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".mtx.xml", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, null); // Removes the ".xml" part of the extension.

                        MtxEncoding encoding;
                        if (options.Fpd is not null)
                        {
                            var fpdFile = new FpdFile(options.Fpd.ToString());
                            encoding = new CharacterMapMtxEncoding(fpdFile.Characters);
                        }
                        else if (options.Fnt is not null)
                        {
                            var fntFile = new FntFile(options.Fnt.ToString());
                            encoding = new CharacterMapMtxEncoding(fntFile.Characters);
                        }
                        else
                        {
                            encoding = new Utf16MtxEncoding();
                        }

                        var serializedSource = File.ReadAllText(filePath);
                        var deserializedSource = Utf8XmlSerializer.Deserialize<MtxSerializable>(serializedSource)
                            ?? throw new NullValueException();
                        var destination = new MtxFile(deserializedSource, encoding);

                        destination.Save(destinationPath);
                    }

                    // XML to CNVRS-TEXT
                    else if (string.Equals(options.Format, "cnvrs-text", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".cnvrs-text.xml", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, null); // Removes the ".xml" part of the extension.

                        var serializedSource = File.ReadAllText(filePath);
                        var deserializedSource = Utf8XmlSerializer.Deserialize<CnvrsTextSerializable>(serializedSource)
                            ?? throw new NullValueException();
                        var destination = new CnvrsTextFile(deserializedSource);

                        destination.Save(destinationPath);
                    }
                }

                // Convert to XML
                else
                {
                    // MTX to XML
                    if (string.Equals(options.Format, "mtx", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".mtx", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, ".mtx.xml");

                        MtxEncoding encoding;
                        if (options.Fpd is not null)
                        {
                            var fpdFile = new FpdFile(options.Fpd.ToString());
                            encoding = new CharacterMapMtxEncoding(fpdFile.Characters);
                        }
                        else if (options.Fnt is not null)
                        {
                            var fntFile = new FntFile(options.Fnt.ToString());
                            encoding = new CharacterMapMtxEncoding(fntFile.Characters);
                        }
                        else
                        {
                            encoding = new Utf16MtxEncoding();
                        }

                        var source = new MtxFile(filePath, encoding);
                        var serializableSource = new MtxSerializable(source);
                        var serializedSource = Utf8XmlSerializer.Serialize(serializableSource);

                        File.WriteAllText(destinationPath, serializedSource);
                    }

                    // CNVRS-TEXT to XML
                    else if (string.Equals(options.Format, "cnvrs-text", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".cnvrs-text", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, ".cnvrs-text.xml");

                        var source = new CnvrsTextFile(filePath);
                        var serializableSource = new CnvrsTextSerializable(source);
                        var serializedSource = Utf8XmlSerializer.Serialize(serializableSource);

                        File.WriteAllText(destinationPath, serializedSource);
                    }

                    // FPD to XML
                    else if (string.Equals(options.Format, "fpd", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".fpd", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, ".fpd.xml");

                        var source = new FpdFile(filePath);
                        var serializableSource = new FpdSerializable(source);
                        var serializedSource = Utf8XmlSerializer.Serialize(serializableSource);

                        File.WriteAllText(destinationPath, serializedSource);
                    }

                    // FNT to XML
                    else if (string.Equals(options.Format, "fnt", StringComparison.OrdinalIgnoreCase)
                        || (options.Format is null && filePath.EndsWith(".fnt", StringComparison.OrdinalIgnoreCase)))
                    {
                        var destinationPath = options.Output is not null
                            ? options.Output.FullName
                            : Path.ChangeExtension(filePath, ".fnt.xml");

                        var source = new FntFile(filePath);
                        var serializableSource = new FntSerializable(source);
                        var serializedSource = Utf8XmlSerializer.Serialize(serializableSource);

                        File.WriteAllText(destinationPath, serializedSource);
                    }
                }
            }
        }
    }
}
