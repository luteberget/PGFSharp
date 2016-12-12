using System;
using System.Linq;

namespace RailCNL2Datalog
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			var stmt = new RailCNL.DistanceObligation(new RailCNL.SubjectClass(new RailCNL.StringClass("balise")),
			  new RailCNL.AnyFound(new RailCNL.AnyDirectionObject(new RailCNL.ObjectClass(new RailCNL.StringClass("signal")))),
			  new RailCNL.Eq(new RailCNL.MkValue(new RailCNL.StringTerm("500")))
			);



			using (var grammar = PGF.Grammar.FromFile ("/home/bjlut/Dropbox/RailCNL.pgf")) {
				var lang = grammar.Languages.First ().Value;
				//var expr = stmt.ToExpression ();
				//var exprStr = expr.ToString ();
				//var exprRead = grammar.ReadExpr (exprStr);

				var input = "avstanden fra en balise til en signal må være 100";
				var parsed = lang.Parse (input).First();
				var outputString = lang.Linearize (parsed);

				var input1 = "en signal er en hovedsignal og en dvergsignal";
				var parsed1 = lang.Parse (input1).First ();
				var statement1 = RailCNL.Statement.FromExpression (parsed1);

				var conv = new Converter ();

				var rules = conv.ConvertStatement (statement1);


				Console.WriteLine (outputString);
			}
		}
	}
}
