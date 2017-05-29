using System;
using System.Collections.Generic;
using System.Linq;

namespace QueryApplication
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var q = "Is 5 prime?";
            Console.WriteLine(q);
            var grammar = PGF.Grammar.FromFile("Query.pgf");
            var language = grammar.Languages.First().Value;
            var input = language.Parse(q).First().Item1;
            var answer = Application.AnswerQuestion(Query.Question.FromExpression(input)).ToExpression();
            var output = language.Linearize(answer);
            Console.WriteLine(output);
        }
    }

    public class Application
    {
        private static bool IsOdd(int x) => x % 2 == 1;
        private static bool IsEven(int x) => x % 2 == 0;
        private static bool IsPrime(int x) => Sieve(Enumerable.Range(2, x)).Contains(x);
        private static IEnumerable<int> Sieve(IEnumerable<int> xs) =>
          !xs.Any() ? Enumerable.Empty<int>() :
            xs.Take(1).Concat(Sieve(xs.Skip(1).Where(n => n % xs.First() > 0)));

        private static Query.Answer Test(Func<int, bool> testF, Query.Object obj) =>
          testF(((Query.Number)obj).x1) ? (Query.Answer)new Query.Yes() : (Query.Answer)new Query.No();

        public static Query.Answer AnswerQuestion(Query.Question q)
        {
            return q.Accept(new Query.Question.Visitor<Query.Answer>(
                VisitPrime: obj => Test(IsPrime, obj),
                VisitOdd: obj => Test(IsOdd, obj),
                VisitEven: obj => Test(IsEven, obj)
            ));
        }
    }
}
