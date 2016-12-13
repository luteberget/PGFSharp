using System;
using System.Collections.Generic;
using System.Linq;

namespace RailCNL2Datalog
{
	using ConjL = System.Collections.Generic.List<RailCNL.Conjunction>;

	public class Converter
	{
		private struct Subject
		{
			public ConjL RulePrefixes;
			public string Variable;
			public string DirVar;
			public RailCNL.Conjunction Dir;
		}

		static readonly string CMP_ERROR_MSG = "Constraint implying a comparison operator is not supported by Datalog output.";
		static readonly string NEGATION_ERROR_MSG = "Constraint implying a negative term is not supported by Datalog output.";
		static readonly string DISJ_ERROR_MSG = "Constraint implying a disjunction is not supported by Datalog output.";

		static readonly string FINDSUFFIX = "found";

		static readonly string PRED_DISTANCE = "distance"; // RailCons
		static readonly string PRED_FOLLOWING = "following"; // RailCons
		static readonly string PRED_DIRECTION = "dir"; // railML
		static readonly string PRED_SWITCH = "switch"; // railML

		int fresh = 0;
		string ruleId;
		public IList<RailCNL.Rule> ConvertStatement (RailCNL.Statement statement, string ruleId)
		{
			this.ruleId = ruleId;
			return statement.Accept (new RailCNL.Statement.Visitor<IList<RailCNL.Rule>> (
				
				VisitAllPathsObligation: (subj,goal,cond) => {
					var subjOut = CreateSubject (subj);
					var goalOut = CreateGoal(subjOut, goal);

					var condOut = cond.Accept(new RailCNL.PathCondition.Visitor<ConjL>(
						VisitPathContains: obj => GetDirObj(subjOut, goalOut.Variable, goalOut.DirVar, obj)
					));

					return null;
				},

				VisitConstraint: (subj, cond) => {
					var subjOut = CreateSubject (subj);
					var consequences = ConditionClauses(subjOut, cond);
					if (consequences.Count == 0) {
						throw new UnsupportedExpressionException ("Statement has no consequences.");
					} else if (consequences.Count > 1) {
						throw new UnsupportedExpressionException (DISJ_ERROR_MSG);
					}
					var outLiterals = ConjToHeads(consequences.Single());

					return RuleCartesian (subjOut.RulePrefixes, outLiterals);
				},

				VisitDistanceObligation: (subj,goal,restr) => {
					var resultType = "distobl";

					var subjOut = CreateSubject(subj);
					var goalOut = CreateGoal(subjOut, goal);

					var distVar = "Dist" + (fresh++).ToString();
					var dist = new List<RailCNL.Conjunction> { Literal3(PRED_DISTANCE, subjOut.Variable, goalOut.Variable, distVar) };
					var okDists = VarRestrictionLiteral(distVar, restr);

					var positiveRuleHead = PropertyLiteral(ruleId + "_" + FINDSUFFIX, subjOut.Variable, goalOut.Variable);
					var negativeRuleHead = PropertyLiteral(ruleId + "_" + resultType, subjOut.Variable, goalOut.Variable);

					var positiveRules =  AndConjunction(AndConjunction(AndConjunction(
						subjOut.RulePrefixes, goalOut.RulePrefixes), dist), okDists).Select(body => 
							(RailCNL.Rule) new RailCNL.MkRule(positiveRuleHead, body));

					var negativeRules = AndConjunction(subjOut.RulePrefixes, goalOut.RulePrefixes).Select( body =>
						(RailCNL.Rule) new RailCNL.MkRule(negativeRuleHead, new RailCNL.Conj(
							body, new RailCNL.Negation(positiveRuleHead))));

					return positiveRules.Concat(negativeRules).ToList();
				},

				VisitObligation:     (subj,cond) => FindResults("obligation",     subj, cond),
				VisitRecommendation: (subj,cond) => FindResults("recommendation", subj, cond)

			));
		}

		// Et signal som har type main eller distant må ha høyde som er større enn 500.
		// --> 
		// error :- signal(X), type(X, main), height(X, Y), Y < 500.
		// error :- signal(X), type(X, distant), height(X, Y), Y < 500.
		//
		// eller
		//
		// ok_signal(X) :- signal(X), type(X, main), height(X,Y), Y > 500.
		// error :- signal(X), type(X,main), !ok_signal(X).
		//
		// Et signal som har type main og distant må ha høyde som er større enn 500 eller mindre enn 100.
		// -->
		// error :- 


		public IList<RailCNL.Rule> FindResults(string resultType, RailCNL.Subject subj, RailCNL.Condition cond) {
			var subjOut = CreateSubject (subj);

			// Create positive rule
			var positive = ConditionClauses(subjOut, cond);
			var product = AndConjunction (subjOut.RulePrefixes, positive);

			var positiveRuleHead = ClassLiteral(ruleId + "_" + FINDSUFFIX, subjOut.Variable);

			var positiveRules = product.Select (body => 
				new RailCNL.MkRule (positiveRuleHead, body)).Cast<RailCNL.Rule>();


			// Find negations.
			var negativeRuleHead = ClassLiteral(ruleId + "_" + resultType, subjOut.Variable);
			var negativeRules = subjOut.RulePrefixes.Select (subjConj =>
				new RailCNL.MkRule (negativeRuleHead, 
				                    new RailCNL.Conj (subjConj,
						new RailCNL.Negation (positiveRuleHead)))).Cast<RailCNL.Rule>();

			// TODO: optimize cases where it is simpler (main signal must have height < 500, this requires only one rule)
			return positiveRules.Concat(negativeRules).ToList();
		}

		public IList<RailCNL.Rule> RuleCartesian(ConjL prefixes, IList<RailCNL.Literal> consequences) {
			var list = new List<RailCNL.Rule> ();

			foreach (var prefix in prefixes) {
				foreach(var consequence in consequences) {
					list.Add (new RailCNL.MkRule (consequence, prefix));
				}
			}

			return list;
		}

		private IList<RailCNL.Literal> ConjToHeads(RailCNL.Conjunction conj) {
			return conj.Accept (new RailCNL.Conjunction.Visitor<IList<RailCNL.Literal>> (
				VisitConj: (c1,c2) => ConjToHeads(c1).Concat(ConjToHeads(c2)).ToList(),
				VisitEqLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG); },
				VisitGteLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG);; },
				VisitGtLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG);; },
				VisitLteLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG);; },
				VisitLtLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG);; },
				VisitNegation: l => { throw new UnsupportedExpressionException(NEGATION_ERROR_MSG); },
				VisitNeqLit: (t1,t2) => { throw new UnsupportedExpressionException(CMP_ERROR_MSG);; },
				VisitSimpleConj: l => new List<RailCNL.Literal> { l } 
			));
		}

		/*
		private IList<RailCNL.Literal> Consequences (Subject subj, RailCNL.Condition cond)
		{

			var consequences = cond.Accept (new RailCNL.Condition.Visitor<List<RailCNL.Literal>>(
				VisitAndCond: (c1, c2) => Consequences (subj, c1).Concat (Consequences (subj, c2)).ToList(),
				VisitConditionClass: cls => cls.Accept (new RailCNL.Class.Visitor<List<RailCNL.Literal>> (
					VisitStringAdjective: null,
					VisitStringClass: str => new List<RailCNL.Literal> {
						new RailCNL.Literal1 (new RailCNL.StringPredicate (str), new RailCNL.StringTerm (subj.Variable))
					})),
				VisitConditionPropertyRestriction: restr => PropertyRestrictionLiterals(restr, subj.Variable),
				VisitDatalogCondition: l => new List<RailCNL.Literal> { l },
				VisitOrCond: (c1,c2) => {
					throw new UnsupportedExpressionException("Constraint implying a disjunction is not supported by Datalog output.");
				}// Not in Datalog!

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

		}*/

		private ConjL ConditionClauses(Subject subj, RailCNL.Condition cond) {
			var consequences = cond.Accept (new RailCNL.Condition.Visitor<ConjL>(
				VisitAndCond: (c1, c2) => AndConjunction(ConditionClauses(subj,c1),ConditionClauses(subj,c2)),
				VisitConditionClass: cls => cls.Accept (new RailCNL.Class.Visitor<ConjL> (
					VisitStringAdjective: null,
					VisitStringClass: str => new List<RailCNL.Conjunction> {
						//new RailCNL.Literal1 (new RailCNL.StringPredicate (str), new RailCNL.StringTerm (subj.Variable))
						ClassLiteralC(str, subj.Variable)
					})),
				VisitConditionPropertyRestriction: restr => PropertyRestrictionLiterals(restr, subj.Variable),
				VisitDatalogCondition: l => new List<RailCNL.Conjunction> { new RailCNL.SimpleConj( l) },
				VisitOrCond: (c1, c2) => OrConjunction(ConditionClauses(subj,c1),ConditionClauses(subj,c2))
			));
			return consequences;		
		}

		private Subject CreateSubject (RailCNL.Subject subj)
		{

			var varName = "Subj" + (fresh++).ToString ();
			var dirVar = "Dir" + (fresh++).ToString ();

			var rulePrefixes = subj.Accept (new RailCNL.Subject.Visitor<ConjL> (
				VisitSubjectClass: cls => SubjectClassLiterals (cls, varName),
				VisitSubjectPropertyRestriction: (cls, propRestr) => {
					var clsConj = SubjectClassLiterals(cls, varName);
					return AndConjunction(clsConj, PropertyRestrictionLiterals(propRestr, varName));
				}));

			return new Subject {
				DirVar = dirVar,
				Dir = PropertyLiteralC(PRED_DIRECTION, varName, dirVar),
				RulePrefixes = rulePrefixes,
				Variable = varName,
			};
		}

		private Subject CreateGoal(Subject subj, RailCNL.GoalObject goal) {
			var goalVar = "Goal" + (fresh++).ToString ();
			var dirVar = "Dir" + (fresh++).ToString ();

			var rulePrefixes = goal.Accept(new RailCNL.GoalObject.Visitor<ConjL>(
				VisitAnyFound: obj => GetDirObj(subj, goalVar, dirVar, obj),
				VisitFirstFound: null
			));

			return new Subject {
				Dir = null,
				DirVar = dirVar,
				RulePrefixes = rulePrefixes,
				Variable = goalVar
			};
		}

		private ConjL GetDirObj(Subject subj, string goalVar, string dirVar, RailCNL.DirectionalObject dirObj) {
			return dirObj.Accept (new RailCNL.DirectionalObject.Visitor<ConjL> (

				VisitAnyDirectionObject: obj => {
					return obj.Accept(new RailCNL.Object.Visitor<ConjL>(
						VisitObjectClass: null,
						VisitObjectPropertyRestriction: null
					));
				},

				VisitFacingSwitch: () => 
				AndConjunction (new List<RailCNL.Conjunction> { ClassLiteralC (PRED_SWITCH, goalVar) },
					AndConjunction (new List<RailCNL.Conjunction> { PropertyLiteralC (PRED_DIRECTION, goalVar, dirVar) },
						new List<RailCNL.Conjunction> { Literal3 (PRED_FOLLOWING, subj.Variable, goalVar, dirVar) })),

				VisitOppositeDirectionObject: null,

				VisitOppositeSearchDirecitonObject: null,

				VisitSameDirectionObject: null,

				VisitSearchDirectionObject: null,

				VisitTrailingSwitch: null

			));
		}

		private ConjL AndConjunction(ConjL l1, ConjL l2) {
			var list = new List<RailCNL.Conjunction> ();
			foreach (var conj1 in l1) {
				foreach(var conj2 in l2) {
					list.Add(JoinConjunctions(conj1,conj2));
				}
			}
			return list;
		}

		private ConjL OrConjunction(ConjL l1, ConjL l2) {
			return l1.Concat (l2).ToList();
		}

		private ConjL PropertyRestrictionLiterals(RailCNL.PropertyRestriction propRestr, string subjVarName) {

			return propRestr.Accept(new RailCNL.PropertyRestriction.Visitor<ConjL> (
				VisitAndPropRestr: (p1, p2) => AndConjunction(PropertyRestrictionLiterals(p1, subjVarName), PropertyRestrictionLiterals(p2, subjVarName)),
				VisitMkPropertyRestriction: (prop,restr) => PropertyRestrictionLiteral(prop,restr, subjVarName),
				VisitOrPropRestr: (p1,p2) => OrConjunction(PropertyRestrictionLiterals(p1, subjVarName), PropertyRestrictionLiterals(p2, subjVarName))
			));
		}

		private string PropertyPredicateString(RailCNL.Property prop) {
			return prop.Accept(new RailCNL.Property.Visitor<string>(
				VisitStringProperty: s => s
			));
		}

		private ConjL AndPropRestr(RailCNL.Property prop, string subjVarName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return AndConjunction (
				PropertyRestrictionLiteral (prop, r1, subjVarName),
				PropertyRestrictionLiteral (prop, r2, subjVarName));
		}

		private ConjL OrPropRestr(RailCNL.Property prop, string subjVarName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return OrConjunction (
				PropertyRestrictionLiteral (prop, r1, subjVarName),
				PropertyRestrictionLiteral (prop, r2, subjVarName));
		}

		private ConjL AndVarRestr(string varName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return AndConjunction (
				VarRestrictionLiteral (varName, r1),
				VarRestrictionLiteral (varName, r2));
		}

		private ConjL OrVarRestr(string varName, RailCNL.Restriction r1, RailCNL.Restriction r2) {
			return OrConjunction (
				VarRestrictionLiteral (varName, r1),
				VarRestrictionLiteral (varName, r2));
		}


		private ConjL VarRestrictionLiteral(string varName, RailCNL.Restriction restr) {
			return restr.Accept(new RailCNL.Restriction.Visitor<ConjL>(

				VisitAndRestr: (r1,r2) => AndVarRestr(varName, r1,r2),

				VisitEq: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.EqLit(t1,t2)),
				VisitGt: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.GtLit(t1,t2)),
				VisitGte: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.GteLit(t1,t2)),
				VisitLt: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.LtLit(t1,t2)),
				VisitLte: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.LteLit(t1,t2)),
				VisitNeq: val => VarRestrCmp(varName, val, (t1,t2) => new RailCNL.NeqLit(t1,t2)),

				VisitOrRestr:  (r1,r2) => OrVarRestr(varName, r1,r2)
			));
		}

		private ConjL PropertyRestrictionLiteral(RailCNL.Property prop, RailCNL.Restriction restr, string subjVarName) {

			var varName = "Val" + (fresh++).ToString ();
			var namedValue = PropertyLiteralC (PropertyPredicateString (prop),
				  subjVarName, varName);

			return restr.Accept(new RailCNL.Restriction.Visitor<ConjL>(
				
				VisitAndRestr: (r1,r2) => AndPropRestr(prop, subjVarName, r1,r2),

				VisitEq: val => new List<RailCNL.Conjunction> { new RailCNL.SimpleConj( new RailCNL.Literal2(
					new RailCNL.StringPredicate(PropertyPredicateString(prop)),
					new RailCNL.StringTerm(subjVarName),
					ValueTerm(val))) },
				
				VisitGt: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.GtLit(t1,t2)),
				VisitGte: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.GteLit(t1,t2)),
				VisitLt: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.LtLit(t1,t2)),
				VisitLte: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.LteLit(t1,t2)),
				VisitNeq: val => PropRestrCmp(varName, val, namedValue, (t1,t2) => new RailCNL.NeqLit(t1,t2)),

				VisitOrRestr:  (r1,r2) => OrPropRestr(prop, subjVarName, r1,r2)
			));
		}

		private  ConjL  PropRestrCmp(string name, RailCNL.Value val, RailCNL.Conjunction named, Func<RailCNL.Term, RailCNL.Term, RailCNL.Conjunction> constructor) {
			return new List<RailCNL.Conjunction> { JoinConjunctions(named, constructor(new RailCNL.StringTerm(name), ValueTerm(val))) };
		}

		private ConjL VarRestrCmp(string var, RailCNL.Value val, Func<RailCNL.Term, RailCNL.Term, RailCNL.Conjunction> constructor) {
			return new List<RailCNL.Conjunction> { constructor(new RailCNL.StringTerm(var), ValueTerm(val)) };
		}

		private RailCNL.Term ValueTerm(RailCNL.Value val) {
			return val.Accept (new RailCNL.Value.Visitor<RailCNL.Term> (
				VisitMkValue: t => t
			));
		}

		private RailCNL.Conjunction JoinConjunctions(RailCNL.Conjunction l1, RailCNL.Conjunction l2) {
			return new RailCNL.Conj (l1, l2);
		}

		private ConjL SubjectClassLiterals(RailCNL.Class cls, string varName) {
			return cls.Accept (new RailCNL.Class.Visitor<ConjL> (
				VisitStringAdjective: null,
				VisitStringClass: str => new List<RailCNL.Conjunction> {
					ClassLiteralC (str, varName)
				}
			));
		}

		private RailCNL.Conjunction ClassLiteralC(string cls, string term) {
			return new RailCNL.SimpleConj(ClassLiteral(cls,term));
		}

		private RailCNL.Literal ClassLiteral(string cls, string term) {
			return new RailCNL.Literal1 (
				new RailCNL.StringPredicate (cls), 
				new RailCNL.StringTerm (term));
		}

		private RailCNL.Literal PropertyLiteral(string prop, string t1, string t2) {
			return new RailCNL.Literal2 (
				new RailCNL.StringPredicate (prop), 
				new RailCNL.StringTerm (t1), 
				new RailCNL.StringTerm (t2));
		}

		private RailCNL.Conjunction PropertyLiteralC(string prop, string t1, string t2) {
			return new RailCNL.SimpleConj(PropertyLiteral(prop,t1,t2));
		}

		private RailCNL.Conjunction Literal3(string prop, string t1, string t2 ,string t3) {
			return new RailCNL.SimpleConj( new RailCNL.Literal3 (
				new RailCNL.StringPredicate (prop), 
				new RailCNL.StringTerm (t1), 
				new RailCNL.StringTerm (t2),
				new RailCNL.StringTerm (t3)));
		}

	}
}

