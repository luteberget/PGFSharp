using System;
using System.Linq;
using System.Collections.Generic;

namespace RailCNL2Datalog
{
	public class EmitDatalog
	{
		public static string Generate(RailCNL.Rule rule) =>
			rule.Accept (new RailCNL.Rule.Visitor<string> (
				VisitMkRule: (head,body) => Glit(head) + " :- " + Gconj(body)
			));

		private static string Gconj(RailCNL.Conjunction c) =>
			c.Accept(new RailCNL.Conjunction.Visitor<string>(
				VisitConj: (c1,c2) => $"{Gconj(c1)}, {Gconj(c2)}",
				VisitEqLit: (t1,t2) => op(@"=", t1,t2),
				VisitGtLit: (t1,t2) => op(@">", t1,t2),
				VisitGteLit: (t1,t2) => op(@">=", t1,t2),
				VisitLtLit: (t1,t2) => op(@"<", t1,t2),
				VisitLteLit: (t1,t2) => op(@"=<", t1,t2),
				VisitNegation: lit => "!" + Glit(lit),
				VisitNeqLit: (t1,t2) => op(@"\=", t1,t2),
				VisitSimpleConj: Glit
			));

		private static string op(string op, RailCNL.Term t1, RailCNL.Term t2) =>
		  String.Join(" ", new string[]{ Gterm(t1), op, Gterm(t2) });

		private static string Glit(RailCNL.Literal l) =>
			l.Accept (new RailCNL.Literal.Visitor<string> (
				VisitLiteral0: p               => GlitList(p, new List<RailCNL.Term> {}),
				VisitLiteral1: (p,t1)          => GlitList(p, new List<RailCNL.Term> {t1}),
				VisitLiteral2: (p,t1,t2)       => GlitList(p, new List<RailCNL.Term> {t1,t2}),
				VisitLiteral3: (p,t1,t2,t3)    => GlitList(p, new List<RailCNL.Term> {t1,t2,t3}),
				VisitLiteral4: (p,t1,t2,t3,t4) => GlitList(p, new List<RailCNL.Term> {t1,t2,t3,t4})
			));

		private static string GlitList (RailCNL.Predicate pred, IList<RailCNL.Term> terms) {
			if (terms.Count == 0)
				return Gpred (pred);
			else 
				return Gpred (pred) + "(" + String.Join (", ", terms.Select(Gterm)) + ")";
		}

		private static string Gpred(RailCNL.Predicate p) =>
			 p.Accept(new RailCNL.Predicate.Visitor<string>(
				VisitStringPredicate: s => s
			));
				

		private static string Gterm(RailCNL.Term t) =>
			t.Accept (new RailCNL.Term.Visitor<string> (
				VisitFloatTerm: f => f.ToString(),
				VisitIntTerm: i => i.ToString(),
				VisitStringTerm: s => s
			));

		private EmitDatalog(){}
	}
}
	