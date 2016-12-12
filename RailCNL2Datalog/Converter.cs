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
				// This is an obligation, so specifies a seach for an object which is neither main or distant.
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
				                   VisitSubjectClass: cls => cls.Accept (new RailCNL.Class.Visitor<IList<RailCNL.Conjunction>> (
					                   VisitStringAdjective: null,
					                   VisitStringClass: str => new List<RailCNL.Conjunction> {
						new RailCNL.SimpleConj (new RailCNL.Literal1 (new RailCNL.StringPredicate (str), new RailCNL.StringTerm (varName)))
					}
				                   )),
				                   VisitSubjectPropertyRestriction: (cls, propRest) => {
					return null;
				}
			                   ));

			return new Subject {
				RulePrefixes = rulePrefixes,
				Variable = varName,
			};
		}
	}
}

