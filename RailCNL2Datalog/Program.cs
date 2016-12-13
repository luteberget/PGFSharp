using System;
using System.Linq;

namespace RailCNL2Datalog
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			using (var grammar = PGF.Grammar.FromFile ("/home/bjlut/Dropbox/RailCNL.pgf")) {
				var lang = grammar.Languages.First ().Value;

				Console.WriteLine ($"-- RailCNL2Datalog: {lang.ToString()}.");
				string inputLine;
				while(true) {
					inputLine = Console.ReadLine ();
					if (inputLine == null)
						break;
					
					try {
						var parsed = lang.Parse(inputLine).First();
						Console.WriteLine($"-- Successfully parsed to: {parsed.ToString()}");
						var statement = RailCNL.Statement.FromExpression(parsed);
						var conv = new Converter();


						var rules = conv.ConvertStatement(statement);
						Console.WriteLine($"-- Parsed statement resulted in {rules.Count} rules.");
						foreach(var rule in rules) {
							Console.WriteLine(EmitDatalog.Generate(rule));
						}
					} catch (PGF.Exceptions.ParseErrorException e) {
						Console.WriteLine("-- Parse failed.");
					}

					Console.WriteLine ();
				}
				Console.WriteLine ("-- Exiting.");
			}
		}
	}
}


/*var stmt = new RailCNL.DistanceObligation(new RailCNL.SubjectClass(new RailCNL.StringClass("balise")),
			  new RailCNL.AnyFound(new RailCNL.AnyDirectionObject(new RailCNL.ObjectClass(new RailCNL.StringClass("signal")))),
			  new RailCNL.Eq(new RailCNL.MkValue(new RailCNL.StringTerm("500")))
			);
*/
