using System;
using System.Collections.Generic;
using System.Linq;

namespace RailCNL2Datalog
{
	public class Converter
	{
		private struct Subject
		{
			public IList<RailCNL.Conjunction> RulePrefixes;
			public string Variable;
		}

		int fresh = 0;

		public IList<RailCNL.Rule> ConvertStatement (RailCNL.Statement statement)
		{
			return statement.Accept (new RailCNL.Statement.Visitor<IList<RailCNL.Rule>> (
				
				VisitAllPathsObligation: null,

				VisitConstraint: (subj, cond) => {
					var subjOut = CreateSubject (subj);
					var consequences = Consequences (subjOut, cond);
					return RuleCartesian (subjOut.RulePrefixes, consequences);
				},

				VisitDistanceObligation: null,

				VisitObligation: null,

				VisitRecommendation: null

			));
		}

		public IList<RailCNL.Rule> RuleCartesian(IList<RailCNL.Conjunction> prefixes, IList<RailCNL.Literal> consequences) {
			var list = new List<RailCNL.Rule> ();

			foreach (var prefix in prefixes) {
				foreach(var consequence in consequences) {
					list.Add (new RailCNL.MkRule (consequence, prefix));
				}
			}

			return list;
		}

		private List<RailCNL.Literal> Consequences (Subject subj, RailCNL.Condition cond)
		{

			var consequences = cond.Accept (new RailCNL.Condition.Visitor<List<RailCNL.Literal>>(
				VisitAndCond: (c1, c2) => Consequences (subj, c1).Concat (Consequences (subj, c2)).ToList(),
				VisitConditionClass: cls => cls.Accept (new RailCNL.Class.Visitor<List<RailCNL.Literal>> (
					VisitStringAdjective: null,
					VisitStringClass: str => new List<RailCNL.Literal> {
						new RailCNL.Literal1 (new RailCNL.StringPredicate (str), new RailCNL.StringTerm (subj.Variable))
					})),
				VisitConditionPropertyRestriction: null, // TODO
				VisitDatalogCondition: l => new List<RailCNL.Literal> { l },
				VisitOrCond: null // Not in Datalog!

				// We should note here that although it is natural to allow expression of the kind
				//  - "{Condition} and {Condition}" 
				//  - "{Condition} or {Condition}"
				// some might actually not be expressible in Datalog. Take the example
				//   > A signal must either be a main signal or a distant signal.
				// This is an obligation, so specifies a search for an object which is neither main nor distant.
				// However, if we change the statement into a *constraint*:
				//   > A signal is either a main signal or a distant signal.
				// It is now a choice statement on the logic level which requires the head of the 
				// rule to have a disjunction. This allows modelling NP-complete problems, and
				// is therefore uses a very different kind of logic and needs a correspondingly different solver.
				// This rule is out of scope for our project.

			                   ));
			return consequences;

		}

		private Subject CreateSubject (RailCNL.Subject subj)
		{

			var varName = "Subj" + (fresh++).ToString ();

			var rulePrefixes = subj.Accept (new RailCNL.Subject.Visitor<IList<RailCNL.Conjunction>> (
				VisitSubjectClass: cls => SubjectClassLiterals (cls, varName),
				VisitSubjectPropertyRestriction: (cls, propRestr) => {
					var clsConj = SubjectClassLiterals(cls, varName);
					return AndConjunction(clsConj, SubjectPropertyRestrictionLiterals(propRestr, varName));
				}));

			return new Subject {
				RulePrefixes = rulePrefixes,
				Variable = varName,
			};
		}

		private IList<RailCNL.Conjunction> AndConjunction(IList<RailCNL.Conjunction> l1, IList<RailCNL.Conjunction> l2) {
			var list = new List<RailCNL.Conjunction> ();
			foreach (var conj1 in l1) {
				foreach(var conj2 in l2) {
					list.Add(JoinConjunctions(conj1,conj2));
				}
			}
			return list;
		}

		private IList<RailCNL.Conjunction> OrConjunction(IList<RailCNL.Conjunction> l1, IList<RailCNL.Conjunction> l2) {
			return l1.Concat (l2).ToList();
		}

		private IList<RailCNL.Conjunction> SubjectPropertyRestrictionLiterals(RailCNL.PropertyRestriction propRestr, string subjVarName) {

			return propRestr.Accept(new RailCNL.PropertyRestriction.Visitor<IList<RailCNL.Conjunction>> (
				VisitAndPropRestr: (p1, p2) => AndConjunction(SubjectPropertyRestrictionLiterals(p1, subjVarName), SubjectPropertyRestrictionLiterals(p2, subjVarName)),
				VisitMkPropertyRestriction: (prop,restr) => PropertyRestrictionLiteral(prop,restr, subjVarName),
				VisitOrPropRestr: (p1,p2) => OrConjunction(SubjectPropertyRestrictionLiterals(p1, subjVarName), SubjectPropertyRestrictionLiterals(p2, subjVarName))
			));
		}

		private string PropertyPredicateString(RailCNL.Property prop) {
			return prop.Accept(new RailCNL.Property.Visitor<string>(
				VisitStringProperty: s => s
			));
		}

		private IList<RailCNL.Conjunction> AndRestriction(RailCNL.Property prop, string subjVarName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return AndConjunction (
				PropertyRestrictionLiteral (prop, r1, subjVarName),
				PropertyRestrictionLiteral (prop, r2, subjVarName));
		}

		private IList<RailCNL.Conjunction> OrRestriction(RailCNL.Property prop, string subjVarName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return OrConjunction (
				PropertyRestrictionLiteral (prop, r1, subjVarName),
				PropertyRestrictionLiteral (prop, r2, subjVarName));
		}

		private IList<RailCNL.Conjunction> PropertyRestrictionLiteral(RailCNL.Property prop, RailCNL.Restriction restr, string subjVarName) {

			var varName = "Val" + (fresh++).ToString ();
			var namedValue = PropertyLiteral (PropertyPredicateString (prop),
				  subjVarName, varName);

			return restr.Accept(new RailCNL.Restriction.Visitor<IList<RailCNL.Conjunction>>(
				
				VisitAndRestr: (r1,r2) => AndRestriction(prop, subjVarName, r1,r2),

				VisitEq: val => new List<RailCNL.Conjunction> { new RailCNL.SimpleConj( new RailCNL.Literal2(
					new RailCNL.StringPredicate(PropertyPredicateString(prop)),
					new RailCNL.StringTerm(subjVarName),
					ValueTerm(val))) },
				
				VisitGt: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.GtLit(t1,t2)),
				VisitGte: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.GteLit(t1,t2)),
				VisitLt: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.LtLit(t1,t2)),
				VisitLte: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.LteLit(t1,t2)),
				VisitNeq: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.NeqLit(t1,t2)),

				VisitOrRestr:  (r1,r2) => OrRestriction(prop, subjVarName, r1,r2)
			));
		}

		private  IList<RailCNL.Conjunction>  PropRestrCmp(string name, RailCNL.Value val, RailCNL.Conjunction named, Func<RailCNL.Term, RailCNL.Term, RailCNL.Conjunction> constructor) {
			return new List<RailCNL.Conjunction> { JoinConjunctions(named, constructor(new RailCNL.StringTerm(name), ValueTerm(val))) };
		}

		private RailCNL.Term ValueTerm(RailCNL.Value val) {
			return val.Accept (new RailCNL.Value.Visitor<RailCNL.Term> (
				VisitMkValue: t => t
			));
		}

		private RailCNL.Conjunction JoinConjunctions(RailCNL.Conjunction l1, RailCNL.Conjunction l2) {
			return new RailCNL.Conj (l1, l2);
		}

		private IList<RailCNL.Conjunction> SubjectClassLiterals(RailCNL.Class cls, string varName) {
			return cls.Accept (new RailCNL.Class.Visitor<IList<RailCNL.Conjunction>> (
				VisitStringAdjective: null,
				VisitStringClass: str => new List<RailCNL.Conjunction> {
					ClassLiteral (str, varName)
				}
			));
		}

		private RailCNL.Conjunction ClassLiteral(string cls, string term) {
					return new RailCNL.SimpleConj( new RailCNL.Literal1 (
				new RailCNL.StringPredicate (cls), 
						new RailCNL.StringTerm (term)));
		}

		private RailCNL.Conjunction PropertyLiteral(string prop, string t1, string t2) {
					return new RailCNL.SimpleConj( new RailCNL.Literal2 (
				new RailCNL.StringPredicate (prop), 
				new RailCNL.StringTerm (t1), 
						new RailCNL.StringTerm (t2)));
		}

	}
}

