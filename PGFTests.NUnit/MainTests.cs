using NUnit.Framework;
using System;
using PGF;
using System.Linq;
namespace PGFTests.NUnit
{
	[TestFixture ()]
	public class MainTests
	{
		[Test]
		public void Canary(){}

		[Test ()]
		public void LoadGrammar ()
		{    
			using (var grammar = Grammar.FromFile ("/home/bjlut/LLCNL/HighLevel.pgf")) 
			{
				var name = grammar.Name;
				var dict = grammar.Languages;

				for (int i = 0; i < 10; i++) {
					foreach (var x in grammar.Languages) {

						Console.WriteLine ("hello");
					}
					foreach (var y in grammar.Categories) {
						Console.WriteLine ("hello");

						foreach (var zz in grammar.FunctionByCategory(y)) {
							
						}

					}
					foreach (var z in grammar.Functions) {

						Console.WriteLine ("hello");


					}
				}
			}
		}

		[Test]
		public void ReadType() {

			using (var grammar = Grammar.FromFile ("/home/bjlut/LLCNL/HighLevel.pgf")) {

				var type_fail = grammar.ReadType ("xyz");
     			var type_succeed = grammar.ReadType ("Constraint");
			}
		}

		[Test]
		public void ReadExpression() {

			using (var grammar = Grammar.FromFile ("/home/bjlut/LLCNL/HighLevel.pgf")) {
				var expr1 = grammar.ReadExpression ("Track");
				var expr2 = grammar.ReadExpression ("(Track)");

				try {
					var expr3 = grammar.ReadExpression ("(");
					Assert.Fail();
				} catch(PGF.Exceptions.ParseErrorException e) {
					// This should fail.
				}

				var expr4 = grammar.ReadExpression ("StringPredicate \"test\"");
			}
		}

		[Test]
		public void ParseTest() {
			using (var grammar = Grammar.FromFile ("/home/bjlut/LLCNL/HighLevel.pgf")) {
				var lang = grammar.Languages.First ().Value;
				var res = lang.Parse ("et spor må være et spor").FirstOrDefault ();

				Assert.True (res.ToString ().StartsWith ("MkPropertyConstraintWImplSubj"));
			}
		}
	}
}

