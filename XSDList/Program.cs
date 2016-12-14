using System;
using CommandLine;
using System.IO;
using System.Xml.Linq;

namespace XSDList
{
	// List

	public class MainClass
	{
		class Options {
			[Option('c', "convert", Required = true,
				HelpText = "Input files to be processed.")]
			public string InputFile { get; set; }

			[Option('o',"output", DefaultValue=null,
				HelpText = "Output file name (default is input filename with .cs extension)")]
			public string OutputFile {get; set;}
		}

		public static void Main (string[] args)
		{
			var options = new Options ();
			var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);
			if (!isValid) {
				Console.WriteLine ("Unknown argument.");
				Console.WriteLine (CommandLine.Text.HelpText.AutoBuild (options));
			} else {
				if(options.OutputFile == null) {
					options.OutputFile = Path.ChangeExtension(options.InputFile, "gf");
				}

				Console.WriteLine($"Loading XSD {options.InputFile}.");

				var xs = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
				// if you have a file: 
				var doc = XDocument.Load(options.InputFile);
				foreach(var element in doc.Descendants(xs + "element"))
				{
					Console.WriteLine(element.Attribute("name").Value);
				}

				Console.WriteLine ("Not writing.QEXit.");
				//File.WriteAllText(options.OutputFile, output);
			}

		}
	}
}
