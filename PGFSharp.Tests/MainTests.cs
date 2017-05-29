using NUnit.Framework;
using System;
using PGF;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace PGFTests.NUnit
{
    [TestFixture()]
    public class MainTests
    {
        private static readonly string grammarFile = Path.Combine(
            TestContext.CurrentContext.TestDirectory, "Foods.pgf");

        [Test, Ignore("Parts of the native library (pgf_generate_all and pgf_compute) might fail with access violations.")]
        public void GenerateAllTest()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var all = grammar.GenerateAll().First();

        }

        [Test, Ignore("Parts of the native library (pgf_generate_all and pgf_compute) might fail with access violations.")]
        public void ExpressionComputeTest()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var expr = grammar.Compute(grammar.ReadExpression("test"));
        }

        [Test]
        public void LinearizeAll()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var lang = grammar.Languages["FoodsEng"];

            var expr = grammar.ReadExpression("Is (That (QKind Fresh Fish)) Italian");
            var outputs = lang.LinearizeAll(expr).ToList();
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("that fresh fish is Italian", outputs[0]);
        }


        [Test]
        public void BracketedLinearise()
        {
            var grammar = Grammar.FromFile(grammarFile);

            var lang = grammar.Languages["FoodsEng"];
            var expr = lang.Parse("these wines are expensive").First().Item1;

            var lin = lang.BracketedLinearize(expr);
            var str = lin.ToString();
            var myStr = lin.ToBracketsString;
            Assert.AreEqual("(Phrase:3 (Item:1 these (Kind:0 wines)) are (Quality:2 expensive))", str);
        }


        [Test]
        public void Canary() { }

        [Test()]
        public void LoadGrammar()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var name = grammar.Name;
            var dict = grammar.Languages;

            for (int i = 0; i < 10; i++)
            {
                foreach (var x in grammar.Languages)
                {
                }
                foreach (var y in grammar.Categories)
                {
                    foreach (var zz in grammar.FunctionByCategory(y))
                    {
                    }

                }
                foreach (var z in grammar.Functions)
                {
                }
            }
        }

        [Test]
        public void ReadType()
        {
            var grammar = Grammar.FromFile(grammarFile);

            var type_which_does_not_exist = grammar.ReadType("xyz");
            Assert.IsNotNull(type_which_does_not_exist);

            var type_which_exists = grammar.ReadType("Constraint");
            Assert.IsNotNull(type_which_exists);
        }

        [Test]
        public void ReadExpression()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var expr1 = grammar.ReadExpression("x");
            var expr2 = grammar.ReadExpression("(x)");
            Assert.AreEqual(expr1.ToString(), expr2.ToString());
            Assert.Throws<PGF.Exceptions.ParseErrorException>(() => grammar.ReadExpression("("));

            var x1 = grammar.ReadExpression("StringPredicate");
            var x2 = grammar.ReadExpression("StringPredicate 1");
            var x3 = grammar.ReadExpression("StringPredicate \"\"");
            var x4 = grammar.ReadExpression("StringPredicate \"x\"");
            var x5 = grammar.ReadExpression("StringPredicate \"æ\"");

            var expr4 = grammar.ReadExpression("StringPredicate \"æøå\"");
            Assert.AreEqual(((expr4 as ApplicationExpression).Argument as LiteralStringExpression).Value, "æøå");
        }

        [Test]
        public void ParseTest()
        {
            var grammar = Grammar.FromFile(grammarFile);
            var lang = grammar.Languages["FoodsEng"];
            var parsedExpr = lang.Parse("these wines are expensive").FirstOrDefault().Item1;
            var cloneExpr = grammar.ReadExpression(parsedExpr.ToString());
            Assert.AreEqual(parsedExpr.ToString(), cloneExpr.ToString());
            Assert.True(parsedExpr.ToString().StartsWith("Is"));

            var qualityType = grammar.ReadType("Quality");
            var boringQuality = lang.Parse("boring", qualityType).First().Item1;
            Assert.AreEqual("Boring", boringQuality.ToString());
        }

        [Test]
        public void LiteralTest()
        {
            var intlit = new LiteralIntExpression(5);
            Assert.AreEqual(5, intlit.Value);
            Assert.AreEqual("5", intlit.ToString());

            var fltlit = new LiteralFloatExpression(9.9);
            Assert.AreEqual(9.9, fltlit.Value);
            Assert.IsTrue(fltlit.ToString().StartsWith("9.9"));

            var strlit = new LiteralStringExpression("hello");
            Assert.AreEqual("hello", strlit.Value);
            Assert.AreEqual("\"hello\"", strlit.ToString());

            //foreach (var l in new Literal[] { intlit, fltlit, strlit })
            //    Assert.IsInstanceOf<Literal>(Expression.FromPtr(l.Ptr, IntPtr.Zero));
        }

        [Test]
        public void MetaVariableTest()
        {
            var meta = new MetaVariableExpression();
            Assert.AreEqual("?", meta.ToString());
            Assert.AreEqual(0, meta.Id);
            //Assert.IsInstanceOf<MetaVariable>(Expression.FromPtr(meta.Ptr, IntPtr.Zero));
        }

        [Test]
        public void ApplicationTest()
        {
            var arg1 = new LiteralIntExpression(1);
            var arg2 = new LiteralStringExpression("xyz");
            var fname = "hello";

            var expr = new ApplicationExpression(fname, new Expression[] { arg1, arg2 });
            Assert.AreEqual("hello 1 \"xyz\"", expr.ToString());
            Assert.IsInstanceOf<ApplicationExpression>(expr.Function);
            Assert.IsInstanceOf<LiteralExpression>(expr.Argument);
            Assert.AreEqual("xyz", (expr.Argument as LiteralStringExpression).Value);
            var inner = expr.Function as ApplicationExpression;

            Assert.IsInstanceOf<FunctionExpression>(inner.Function);
            var fun = inner.Function as FunctionExpression;
            Assert.AreEqual("hello", fun.Name);

            Assert.AreEqual(1, (inner.Argument as LiteralIntExpression).Value);
        }

        [Test]
        public void NullaryFunctionIsApplication()
        {

            var grammar = Grammar.FromFile(grammarFile);
            var expr1 = grammar.ReadExpression("asdfasdf asd");

            //Assert.IsInstanceOf<Function> (expr1);
            Assert.IsInstanceOf<ApplicationExpression>(expr1);

            var expr2 = grammar.ReadExpression("asdf (xaxa lala) (xaxa lala kaka)");

            var vstr = new Expression.Visitor<string>();
            Func<IEnumerable<Expression>, IEnumerable<string>> ag = args => args.Select(a => a.Accept(vstr));
            vstr.fVisitApplication = (fn, args) => $"_{fn}({String.Join(",", ag(args))})";

            var outString2 = expr2.Accept(vstr);

            //Assert.AreEqual ("_asdf", outString);
            Assert.AreEqual("_asdf(_xaxa(_lala()),_xaxa(_lala(),_kaka()))", outString2);
        }
    }
}

