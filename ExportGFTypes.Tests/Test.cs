using NUnit.Framework;
using System;

namespace ExportGFTypes.NUnit
{
	[TestFixture ()]
	public class Test
	{

		[Test]
		public void CodeGeneratorTest() {

			var abs = new CodeGenerator.Grammar {
				Name = "Query",
				Categories = new System.Collections.Generic.List<CodeGenerator.Category> {
					new CodeGenerator.Category {
						Name = "Answer",

						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Yes",
								ArgumentTypes = new System.Collections.Generic.List<string> { }
							},
							new CodeGenerator.Constructor {
								Name = "No",
								ArgumentTypes = new System.Collections.Generic.List<string> { }
							},
						},
					},
					new CodeGenerator.Category {
						Name = "Object",
						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Number",
								ArgumentTypes = new System.Collections.Generic.List<string> {
									"Int"
								},
							}
						}
					},
					new CodeGenerator.Category {
						Name = "Question",
						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Even",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
							new CodeGenerator.Constructor {
								Name = "Odd",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
							new CodeGenerator.Constructor {
								Name = "Prime",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
						}
					}
				}
			};

			var output = CodeGenerator.ToString (abs);
            var expected = @"public abstract class Tree {
  public abstract PGF.Expression ToExpression();
}

namespace Query {
  public abstract class Answer : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitYes();
      R VisitNo();
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<R> _VisitYes { get; set; }
      private System.Func<R> _VisitNo { get; set; }
      public Visitor(System.Func<R> VisitYes, System.Func<R> VisitNo) {
        this._VisitYes = VisitYes;
        this._VisitNo = VisitNo;
      }
      
      public R VisitYes() => _VisitYes();
      public R VisitNo() => _VisitNo();
    }
    
    public static Answer FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Answer>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(Yes) && args.Length == 0)
            return new Yes();
          if(fname == nameof(No) && args.Length == 0)
            return new No();
          throw new System.ArgumentOutOfRangeException();
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
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Yes), new PGF.Expression[]{});
    }
    
  }
  
  public class No : Answer {
    public No() {
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitNo();
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(No), new PGF.Expression[]{});
    }
    
  }
  
  public abstract class Object : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitNumber(int x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<int,R> _VisitNumber { get; set; }
      public Visitor(System.Func<int,R> VisitNumber) {
        this._VisitNumber = VisitNumber;
      }
      
      public R VisitNumber(int x1) => _VisitNumber(x1);
    }
    
    public static Object FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Object>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(Number) && args.Length == 1)
            return new Number(((PGF.LiteralInt)args[0]).Value);
          throw new System.ArgumentOutOfRangeException();
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
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Number), new PGF.Expression[]{new PGF.LiteralInt(x1)});
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
      private System.Func<Query.Object,R> _VisitEven { get; set; }
      private System.Func<Query.Object,R> _VisitOdd { get; set; }
      private System.Func<Query.Object,R> _VisitPrime { get; set; }
      public Visitor(System.Func<Query.Object,R> VisitEven, System.Func<Query.Object,R> VisitOdd, System.Func<Query.Object,R> VisitPrime) {
        this._VisitEven = VisitEven;
        this._VisitOdd = VisitOdd;
        this._VisitPrime = VisitPrime;
      }
      
      public R VisitEven(Query.Object x1) => _VisitEven(x1);
      public R VisitOdd(Query.Object x1) => _VisitOdd(x1);
      public R VisitPrime(Query.Object x1) => _VisitPrime(x1);
    }
    
    public static Question FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Question>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(Even) && args.Length == 1)
            return new Even(Object.FromExpression(args[0]));
          if(fname == nameof(Odd) && args.Length == 1)
            return new Odd(Object.FromExpression(args[0]));
          if(fname == nameof(Prime) && args.Length == 1)
            return new Prime(Object.FromExpression(args[0]));
          throw new System.ArgumentOutOfRangeException();
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
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Even), new PGF.Expression[]{x1.ToExpression()});
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
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Odd), new PGF.Expression[]{x1.ToExpression()});
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
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Prime), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
}

";
            Assert.AreEqual(expected, output);
        }
	}
}

