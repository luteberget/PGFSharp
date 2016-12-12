using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PGF;
using CommandLine;


namespace ExportGFTypes
{
	public class MainClass {
		class Options {
			[Option('c', "convert", Required = true,
				HelpText = "Input files to be processed.")]
			public string InputFile { get; set; }

			[Option('o',"output", DefaultValue=null,
				HelpText = "Output file name (default is input filename with .cs extension)")]
				public string OutputFile {get; set;}

			/*
			// Omitting long name, default --verbose
			[Option(
				HelpText = "Prints all messages to standard output.")]
			public bool Verbose { get; set; }

			[Option(Default = "中文",
				HelpText = "Content language.")]
			public string Language { get; set; }

			[Value(0, MetaName = "offset",
				HelpText = "File offset.")]
			public long? Offset { get; set; }
			*/
		}

		public static void Main(string[] args) {
			//Console.WriteLine ($"ARGS:\n  {String.Join("\n  ", args)}");
			var options = new Options ();
			var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);
			if (!isValid) {
				Console.WriteLine ("Unknown argument.");
				Console.WriteLine (CommandLine.Text.HelpText.AutoBuild (options));
			} else {
				if(options.OutputFile == null) {
					options.OutputFile = Path.ChangeExtension(options.InputFile, "cs");
				}

				Console.WriteLine($"Loading PGF {options.InputFile}.");

				string output;

				using(var grammar = Grammar.FromFile(options.InputFile)) {
					Console.WriteLine($"{grammar.ToString()}.");

					Console.WriteLine($"Converting to code generator input.");
					var grammarInput = PGFGrammarInput.Convert(grammar);
					Console.WriteLine($"Calling code generator.");
					output = CodeGenerator.ToString(grammarInput);

					Console.WriteLine($"Closing PGF.");
				}

				Console.WriteLine($"Writing to {options.OutputFile}.");
				File.WriteAllText(options.OutputFile, output);
			}
		}
	}
}

