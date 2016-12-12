using System;
using PGF;
public abstract class Tree {
	public abstract Expression ToExpression();
}

namespace Query {
	public abstract class Answer : Tree {
		public abstract R Accept<R>(IVisitor<R> visitor);
		public interface IVisitor<R> {
			R VisitYes();
			R VisitNo();
		}

		public class Visitor<R> : IVisitor<R> {
			private Func<R> _VisitYes { get; set; }
			private Func<R> _VisitNo { get; set; }
			public Visitor(Func<R> VisitYes, Func<R> VisitNo) {
				this._VisitYes = VisitYes;
				this._VisitNo = VisitNo;
			}

			public R VisitYes() => _VisitYes();
			public R VisitNo() => _VisitNo();
		}

		public static Answer FromExpression(Expression expr) {
			return expr.Accept(new Expression.Visitor<Answer>() {
				fVisitApplication = (fname,args) =>  {
					if(fname == nameof(Yes) && args.Length == 0)
						return new Yes();
					if(fname == nameof(No) && args.Length == 0)
						return new No();
					throw new ArgumentOutOfRangeException();
				}

			});

		}

	}

	public class Yes : Answer {
		public Yes() {
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitYes();
		}

		public override Expression ToExpression() {
			return new Application(nameof(Yes), new Expression[]{});
		}

	}

	public class No : Answer {
		public No() {
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitNo();
		}

		public override Expression ToExpression() {
			return new Application(nameof(No), new Expression[]{});
		}

	}

	public abstract class Object : Tree {
		public abstract R Accept<R>(IVisitor<R> visitor);
		public interface IVisitor<R> {
			R VisitNumber(int x1);
		}

		public class Visitor<R> : IVisitor<R> {
			private Func<int,R> _VisitNumber { get; set; }
			public Visitor(Func<int,R> VisitNumber) {
				this._VisitNumber = VisitNumber;
			}

			public R VisitNumber(int x1) => _VisitNumber(x1);
		}

		public static Object FromExpression(Expression expr) {
			return expr.Accept(new Expression.Visitor<Object>() {
				fVisitApplication = (fname,args) =>  {
					if(fname == nameof(Number) && args.Length == 1)
						return new Number((int)(((LiteralInt)args[0]).Value));
					throw new ArgumentOutOfRangeException();
				}

			});

		}

	}

	public class Number : Object {
		public int x1 {get; set;}
		public Number(int x1) {
			this.x1 = x1;
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitNumber(x1);
		}

		public override Expression ToExpression() {
			return new Application(nameof(Number), new Expression[]{new LiteralInt(x1)});
		}

	}

	public abstract class Question : Tree {
		public abstract R Accept<R>(IVisitor<R> visitor);
		public interface IVisitor<R> {
			R VisitEven(Query.Object x1);
			R VisitOdd(Query.Object x1);
			R VisitPrime(Query.Object x1);
		}

		public class Visitor<R> : IVisitor<R> {
			private Func<Query.Object,R> _VisitEven { get; set; }
			private Func<Query.Object,R> _VisitOdd { get; set; }
			private Func<Query.Object,R> _VisitPrime { get; set; }
			public Visitor(Func<Query.Object,R> VisitEven, Func<Query.Object,R> VisitOdd, Func<Query.Object,R> VisitPrime) {
				this._VisitEven = VisitEven;
				this._VisitOdd = VisitOdd;
				this._VisitPrime = VisitPrime;
			}

			public R VisitEven(Query.Object x1) => _VisitEven(x1);
			public R VisitOdd(Query.Object x1) => _VisitOdd(x1);
			public R VisitPrime(Query.Object x1) => _VisitPrime(x1);
		}

		public static Question FromExpression(Expression expr) {
			return expr.Accept(new Expression.Visitor<Question>() {
				fVisitApplication = (fname,args) =>  {
					if(fname == nameof(Even) && args.Length == 1)
						return new Even(Object.FromExpression(args[0]));
					if(fname == nameof(Odd) && args.Length == 1)
						return new Odd(Object.FromExpression(args[0]));
					if(fname == nameof(Prime) && args.Length == 1)
						return new Prime(Object.FromExpression(args[0]));
					throw new ArgumentOutOfRangeException();
				}

			});

		}

	}

	public class Even : Question {
		public Query.Object x1 {get; set;}
		public Even(Query.Object x1) {
			this.x1 = x1;
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitEven(x1);
		}

		public override Expression ToExpression() {
			return new Application(nameof(Even), new Expression[]{x1.ToExpression()});
		}

	}

	public class Odd : Question {
		public Query.Object x1 {get; set;}
		public Odd(Query.Object x1) {
			this.x1 = x1;
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitOdd(x1);
		}

		public override Expression ToExpression() {
			return new Application(nameof(Odd), new Expression[]{x1.ToExpression()});
		}

	}

	public class Prime : Question {
		public Query.Object x1 {get; set;}
		public Prime(Query.Object x1) {
			this.x1 = x1;
		}

		public override R Accept<R>(IVisitor<R> visitor) {
			return visitor.VisitPrime(x1);
		}

		public override Expression ToExpression() {
			return new Application(nameof(Prime), new Expression[]{x1.ToExpression()});
		}

	}

}

