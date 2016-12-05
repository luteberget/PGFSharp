# Grammatical Framework PGF library .NET wrapper

This project contains three parts:

1. A C# library which wraps the `pgf` and `gu` native libraries
2. A patch and a CMake configuration for the GF C runtime, allowing it to be built with the MSVC.
3. A code generator which generates code for representing PGF categories and constructors as C# classes. 
   The classes can be constructed from PGF expressions and converted back into PGF expressions.

Together, these part produce a very lightweight PGF library which can be used in Windows desktop applications.

## Development status
The library can be used for basic PGF applications, but some work remains:

 * Some memory management issues remain, notably in the clean-up of the `Expression` class. Will use the Python wrapper as example of how this should be done.
 * Callback noun functions from parser are not yet supported, should be easy.
 * Parser sentence completion (predictive parsing).
 * Load/unload languages.

The following items are **not** planned for inclusion.

 * Higher-order and dependent types can be used through the basic inteface, but the `Expression` class does not support manipulation of them.
 * Representing type information in C#.
 * Evaluation of parses (not planned for implementation).
 * Tabular linearize.
 * Bracketed linearize.
 * Graphviz generation.
 * Full form lexicon.
 * Morphological lookup.
 * Type-check expression.

## An application using the 

Following the example in the 
[Grammatical Framework Tutorial](http://www.grammaticalframework.org/doc/tutorial/gf-tutorial.html#toc151) 
about building a translation application by calling the PGF library from
another programming language (Haskell, in the case of the tutorial),
we show how the C# PGF code generator can be used to simplify the development
of a **transfer** function used in a query answering application.

The following abstract syntax 

```haskell
  abstract Query = {

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
```

 To make it easy to define a transfer function, we export the abstract syntax to a system of C# classes: 

       > pgf2cs.exe Query.pgf

The result is a file named `Query.cs` containing a namespace named `Query` which contains abstract classes for each category in the PGF, and concrete classes
for each constructor. 

```csharp
namespace Query {
  public abstract class Answer {
    public abstract Expression ToExpression();
    public static Answer FromExpression(Expression expr) { ... }
    public abstract R Accept<R>(IVisitor <R> visitor);
    public interface IVisitor<R> {
      R VisitYes();
      R VisitNo();
    }
  }

  public class Yes : Answer {
    public Yes() { }
    public override Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitYes();
    }
    public override Expression ToExpression() { ... }
  }

  ...
}
```


Now, we can use these classes to write an application, and the C# type system
ensures that the manipulations by the transfer function produce well-formed 
syntax trees.

```csharp
namespace QueryApplication
{
  class MainClass
  {
    public static void Main (string[] args)
    {
      var q = "Is 5 prime?";
      Console.WriteLine (q);
      using (var grammar = PGF.Grammar.FromFile ("Query.pgf")) {
        var language = grammar.Languages.First ().Value;
        var input = language.Parse (q).First();
        var answer = Application.AnswerQuestion (Query.Question.FromExpression (input)).ToExpression ();
        var output = language.Linearize (answer);
        Console.WriteLine (output);
      }
    }
  }

  public class Application {
    private static bool IsOdd (int x) => x % 2 == 1;
    private static bool IsEven (int x) => x % 2 == 0;
    private static bool IsPrime (int x) => Sieve(Enumerable.Range(2,x)).Contains(x);
    private static IEnumerable<int> Sieve(IEnumerable<int> xs) => 
      !xs.Any() ? Enumerable.Empty<int>() :
        xs.Take(1).Concat(Sieve(xs.Skip(1).Where(n => n % xs.First() > 0)));

    private static Query.Answer Test (Func<int, bool> testF, Query.Object obj) =>
      testF(((Query.Number)obj).x1) ? (Query.Answer) new Query.Yes() : (Query.Answer) new Query.No();

    public static Query.Answer AnswerQuestion(Query.Question q) {
      return q.Accept(new Query.Question.Visitor<Query.Answer>(
        VisitPrime: obj => Test(IsPrime, obj),
        VisitOdd: obj => Test(IsOdd, obj),
        VisitEven: obj => Test(IsEven, obj)
      ));
    }
  }
}

```
