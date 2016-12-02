using NUnit.Framework;
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


			var _pool = NativeGU.gu_new_pool ();
			IntPtr _expr = IntPtr.Zero;

			var exprTag = (byte)(int)Expression.PgfExprTag.PGF_EXPR_LIT;
			IntPtr litPtr = NativeGU.gu_alloc_variant (exprTag, 
				(UIntPtr)Marshal.SizeOf<Literal.NativePgfExprLit>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<Literal.NativePgfExprLit> (litPtr, lit => {
				var litTag = (byte)(int)Literal.NativePgfLiteralTag.PGF_LITERAL_INT;
				IntPtr ilitPtr = NativeGU.gu_alloc_variant (litTag,
					(UIntPtr)Marshal.SizeOf<Literal.NativePgfLiteralInt> (), UIntPtr.Zero, ref lit.lit, _pool);
				Native.EditStruct<Literal.NativePgfLiteralInt>(ilitPtr, ilit => { ilit.val = 55; return ilit; }); 
				return lit;
			});

			//var lit = new Literal ();


			var lit2 = new Literal (5);

			//var str = lit.ToString ();

			//Assert.AreSame ("?", str);

			var no = lit2.Value;

			var str2 = lit2.ToString ();

			Assert.AreEqual ("5", str2);

		}


		[Test]
		public void EnumTest() {
			var asInt = (int)(Expression.PgfExprTag.PGF_EXPR_LIT);
			var asByte = (byte)(Expression.PgfExprTag.PGF_EXPR_LIT);
			var asByte2 = (byte)((int)(Expression.PgfExprTag.PGF_EXPR_LIT));

		}
	}
}

