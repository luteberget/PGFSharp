using System;
using System.Collections.Generic;
using System.Linq;
using PGF;
using ExportGFTypes.Query;
namespace ExportGFTypes
{

	// Generate C# classes corresponding to a GF grammar.
	// Incomplete, does not support:
	//  - Higher-order and dependent types
	//  - List types

	public class MyClass
	{
		public MyClass ()
		{
			//var ans = Query.Answer.FromExpression (Expression.FromString("Yes"));
			var x = Query.Answer.FromExpression(null);

		}
	}

	// GF tutorial: putting it all together (answer query function)


	public class Application {
		
		public static bool IsOdd (int x) => x % 2 == 1;
		public static bool IsEven (int x) => x % 2 == 0;
		public static bool IsPrime (int x) => Sieve(Enumerable.Range(2,x)).Contains(x);
		public static IEnumerable<int> Sieve(IEnumerable<int> xs) => 
		  !xs.Any() ? Enumerable.Empty<int>() :
		    xs.Take(1).Concat(Sieve(xs.Skip(1).Where(n => n % xs.First() > 0)));

		static void Main() {
			var grammar = Grammar.FromFile ("");
			var language = grammar.Languages.Values.First ();

			language.Parse ("Is 5 prime?");

			string input = null;
			Expression inputExpr = null; // Parse expression
			Expression inputExpr2 = null; // Parse concerte language

			Query.Question question = Query.Question.FromExpression (inputExpr);


			var answer = Transfer (Query.Question.FromExpression (language.Parse ("Is 5 prime?").First()));
			var answerText = language.Linearize (answer.ToExpression ());

			// Do something

			/*Query.Answer answer = Transfer (question);
			var outputExpr = answer.ToExpression ();*/
		}

		/*private class QuestionAnswerVisitor : Query.Question.Visitor<Query.Answer> {
			public Query.Answer VisitPrime (Query.Object Object0) => Test (IsPrime, Object0);
			public Query.Answer VisitOdd (Query.Object Object0) => Test (IsOdd, Object0);
			public Query.Answer VisitEven (Query.Object Object0) => Test (IsEven, Object0);
		}*/

		private static Answer Transfer(Question q) {
			//return q.Accept(new QuestionAnswerVisitor ());
			return q.Accept(new Question.Visitor<Answer>(
				VisitPrime: obj => Test(IsPrime, obj),
				VisitOdd: obj => Test(IsOdd, obj),
				VisitEven: obj => Test(IsEven, obj)
			));
		}

		/*private class ObjectNumberVisitor : Query.Object.Visitor<int> {
			public int VisitNumber (int Int0) => Int0;
		}*/

		private static int IntValue(Query.Object o) {
			return o.Accept (new Query.Object.Visitor<int>(i => i));
		}

		private static Answer Test (Func<int, bool> testF, Query.Object obj) =>
		  testF(IntValue(obj)) ? (Answer) new Yes() : (Answer) new No();

		/*{
			if(test(Value(obj))) {
				return new Yes();
			} else {
				return new No();
			}
		}*/
		  //test(Value(obj)) ? (Query.Answer) new Query.Yes() : (Query.Answer) new Query.No();
	}

/*
 * 
 *   abstract Query = {

    flags startcat=Question ;

    cat
      Answer ; Question ; Object ;

    fun
      Even   : Object -> Question ;
      Odd    : Object -> Question ;
      Prime  : Object -> Question ;
      Number : Int -> Object ;

      Yes : Answer ;
      No  : Answer ;
  }
*/

	// Each category is a class.
	// Each 

	public abstract class Tree {
		

		public abstract string ToExpressionString();

		public virtual Expression ToExpression() {
			throw new NotImplementedException ();
			///return ToExpressionString ();
		}

		public abstract string ClassName { get;}

		// TODO: move to 
		protected string MkCId(string s) => "(" + s + ")";
		protected string MkApp(string CId, IEnumerable<string> args)  => "(" + CId + " " + String.Join(" ", args) +  ")";
	}

	namespace Query {
		public abstract class Answer : Tree {
			public abstract R Accept<R>(IVisitor<R> visitor);
			public interface IVisitor<R> {
				R VisitYes();
				R VisitNo();
			}

			public class Visitor<R> : IVisitor<R>{
				private Func<R> _VisitYes { get; set; }
				private Func<R> _VisitNo { get; set; }


				public Visitor(Func<R> VisitYes, Func<R> VisitNo) {
					this._VisitYes = VisitYes;
					this._VisitNo = VisitNo;
				}

				public R VisitYes() => _VisitYes();
				public R VisitNo() => _VisitYes();
			}

			public static Answer FromExpression(Expression expr) {
				//var head = expr.UnApp ();
				string head = "";
				if (head == "Yes") new Yes ();
				if (head == "No") new No ();
				return null;
			}
		}

		public class Yes : Answer { 

			public override string ClassName => nameof(Yes);
			public override string ToExpressionString () => ClassName;

			public override R  Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitYes();
			}

		}
		public class No : Answer {public override string ClassName => nameof(No);
			public override string ToExpressionString () => ClassName;
			public override R  Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitNo();
			}
		}

		public abstract class Object : Tree {

			public interface IVisitor<R> {
				R VisitNumber(int Int0);
			}
			public abstract R Accept<R>(IVisitor<R> visitor);

			public static Object FromExpression(Expression expr) {
				return expr.Accept(new Expression.Visitor<Object> {
					fVisitApplication = (fn, args) => {
						if(fn == "Number" && args.Length == 1) {
							return new Number((int)(args.First() as Literal).Value);
						}
						throw new NotImplementedException();
					}
				});
			}

			public class Visitor<R> : IVisitor<R>{
				private Func<int,R> _VisitNumber { get; set; }

				public Visitor(Func<int,R> VisitNumber) {
					this._VisitNumber = VisitNumber;
				}

				public R VisitNumber(int Int0) => _VisitNumber(Int0);
			}
		}

		public class Number : Object {
			public override string ClassName => nameof(Number);

			public override string ToExpressionString() => ToExpression().ToString();

			public override Expression ToExpression ()
			{
				return new PGF.Application("Number", new[]{new Literal(Int0)});
			}

			public int Int0 { get; set;}
			public Number(int Int0) {
				this.Int0 = Int0;
			}

			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitNumber(Int0);
			}
		}

		public abstract class Question :Tree {
			public interface IVisitor<R> {
				R VisitPrime(Object Object0);
				R VisitOdd(Object Object0);
				R VisitEven(Object Object0);
			}
			public abstract R Accept<R>(IVisitor<R> visitor);

			public static Question FromExpression(Expression expr) {
				//var unApp = expr.UnApp ();
				string unApp = null;

				if (unApp == "Prime x")
					return new Prime ( Number.FromExpression(null) );
				if (unApp == "Odd x")
					return new Odd (Number.FromExpression (null));
				if (unApp == "Even x")
					return new Even (Number.FromExpression (null));

				throw new ArgumentOutOfRangeException ();
			}

			public class Visitor<R> : IVisitor<R>{
				private Func<Object,R> _VisitPrime { get; set; }
				private Func<Object,R> _VisitOdd { get; set; }
				private Func<Object,R> _VisitEven { get; set; }

				public Visitor(
					Func<Object,R> VisitPrime,
					Func<Object,R> VisitOdd,
					Func<Object,R> VisitEven
				) {
					this._VisitPrime = VisitPrime;
					this._VisitOdd = VisitOdd;
					this._VisitEven = VisitEven;
				}

				public R VisitPrime(Object Object0) => _VisitPrime(Object0);
				public R VisitOdd(Object Object0) => _VisitOdd(Object0);
				public R VisitEven(Object Object0) => _VisitEven(Object0);
			}
		}

		public class Prime :Question {
			public override string ClassName => nameof(Prime);
			public override string ToExpressionString ()
			{
				return MkApp(nameof(Prime), new[]{Object0}.Select(x => x.ToExpressionString()));
			}

			public Object Object0 {get;set;}
			public Prime(Object Object0) { this.Object0 = Object0;}
			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitPrime(Object0);
			}
		}

		public class Odd :Question {
			public override string ClassName => nameof(Odd );
			public override string ToExpressionString ()
			{
				return MkApp(ClassName, new[]{Object0}.Select(x => x.ToExpressionString()));
			}
			public Object Object0 {get;set;}
			public Odd(Object Object0) { this.Object0 = Object0;}
			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitOdd(Object0);
			}
		}

		public class Even :Question {
			public override string ClassName => nameof(Even);
			public override string ToExpressionString ()
			{
				return MkApp(ClassName, new[]{Object0}.Select(x => x.ToExpressionString()));
			}
			public Object Object0 {get;set;}
			public Even(Object Object0) { this.Object0 = Object0;}
			public override R Accept<R>(IVisitor<R> visitor) {
				return visitor.VisitEven(Object0);
			}
		}
	}
}

