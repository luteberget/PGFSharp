﻿using NUnit.Framework;
using System;
using PGF;
using System.Linq;
using System.Runtime.InteropServices;


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

		[Test]
		public void LiteralTest() {
			var intlit = new Literal (5);
			Assert.AreEqual (5, intlit.Value);
			Assert.AreEqual ("5", intlit.ToString ());

			var fltlit = new Literal (9.9);
			Assert.AreEqual (9.9, fltlit.Value);
			Assert.IsTrue (fltlit.ToString ().StartsWith ("9,9"));

			var strlit = new Literal ("hello");
			Assert.AreEqual ("hello", strlit.Value);
			Assert.AreEqual ("\"hello\"", strlit.ToString ());

			foreach(var l in new[]{ intlit, fltlit, strlit})
			  Assert.IsInstanceOf<Literal> (Expression.FromPtr (l.NativePtr, IntPtr.Zero));
		}

		[Test]
		public void MetaVariableTest() {
			var meta = new MetaVariable ();
			Assert.AreEqual ("?", meta.ToString ());
			Assert.AreEqual (0, meta.Id);
			Assert.IsInstanceOf<MetaVariable> (Expression.FromPtr (meta.NativePtr, IntPtr.Zero));
		}

		[Test]
		public void ApplicationTest() {
			var arg1 = new Literal (1);
			var arg2 = new Literal ("xyz");
			var fname = "hello";

			var expr = new Application (fname, new[]{ arg1, arg2 });
			Assert.AreEqual ("hello 1 \"xyz\"", expr.ToString ());
			Assert.IsInstanceOf<Application> (expr.Function);
			Assert.IsInstanceOf<Literal> (expr.Argument);
			Assert.AreEqual ("xyz", (expr.Argument as Literal).Value);
			var inner = expr.Function as Application;

			Assert.IsInstanceOf<Function> (inner.Function);
			var fun = inner.Function as Function;
			Assert.AreEqual ("hello", fun.Name);

			Assert.AreEqual (1, (inner.Argument as Literal).Value);
		}


		[Test]
		public void EnumTest() {
			var asInt = (int)(Expression.PgfExprTag.PGF_EXPR_LIT);
			var asByte = (byte)(Expression.PgfExprTag.PGF_EXPR_LIT);
			var asByte2 = (byte)((int)(Expression.PgfExprTag.PGF_EXPR_LIT));

		}

		[Test]
		public void NullaryFunctionIsApplication() {

			using (var grammar = Grammar.FromFile ("/home/bjlut/LLCNL/HighLevel.pgf")) {
				var expr1 = grammar.ReadExpression ("asdfasdf asd");

				Assert.IsInstanceOf<Function> (expr1);
				Assert.IsInstanceOf<Application> (expr1);
			}
		}
	}
}

