public abstract class Tree {
  public abstract PGF.Expression ToExpression();
}

namespace RailCNL {
  public abstract class Class : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitStringAdjective(string x1,RailCNL.Class x2);
      R VisitStringClass(string x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<string,RailCNL.Class,R> _VisitStringAdjective { get; set; }
      private System.Func<string,R> _VisitStringClass { get; set; }
      public Visitor(System.Func<string,RailCNL.Class,R> VisitStringAdjective, System.Func<string,R> VisitStringClass) {
        this._VisitStringAdjective = VisitStringAdjective;
        this._VisitStringClass = VisitStringClass;
      }
      
      public R VisitStringAdjective(string x1,RailCNL.Class x2) => _VisitStringAdjective(x1, x2);
      public R VisitStringClass(string x1) => _VisitStringClass(x1);
    }
    
    public static Class FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Class>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(StringAdjective) && args.Length == 2)
            return new StringAdjective(((PGF.LiteralString)args[0]).Value, Class.FromExpression(args[1]));
          if(fname == nameof(StringClass) && args.Length == 1)
            return new StringClass(((PGF.LiteralString)args[0]).Value);
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class StringAdjective : Class {
    public string x1 {get; set;}
    public RailCNL.Class x2 {get; set;}
    public StringAdjective(string x1, RailCNL.Class x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitStringAdjective(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(StringAdjective), new PGF.Expression[]{new PGF.LiteralString(x1), x2.ToExpression()});
    }
    
  }
  
  public class StringClass : Class {
    public string x1 {get; set;}
    public StringClass(string x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitStringClass(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(StringClass), new PGF.Expression[]{new PGF.LiteralString(x1)});
    }
    
  }
  
  public abstract class Condition : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAndCond(RailCNL.Condition x1,RailCNL.Condition x2);
      R VisitConditionClass(RailCNL.Class x1);
      R VisitConditionPropertyRestriction(RailCNL.PropertyRestriction x1);
      R VisitDatalogCondition(RailCNL.Literal x1);
      R VisitOrCond(RailCNL.Condition x1,RailCNL.Condition x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Condition,RailCNL.Condition,R> _VisitAndCond { get; set; }
      private System.Func<RailCNL.Class,R> _VisitConditionClass { get; set; }
      private System.Func<RailCNL.PropertyRestriction,R> _VisitConditionPropertyRestriction { get; set; }
      private System.Func<RailCNL.Literal,R> _VisitDatalogCondition { get; set; }
      private System.Func<RailCNL.Condition,RailCNL.Condition,R> _VisitOrCond { get; set; }
      public Visitor(System.Func<RailCNL.Condition,RailCNL.Condition,R> VisitAndCond, System.Func<RailCNL.Class,R> VisitConditionClass, System.Func<RailCNL.PropertyRestriction,R> VisitConditionPropertyRestriction, System.Func<RailCNL.Literal,R> VisitDatalogCondition, System.Func<RailCNL.Condition,RailCNL.Condition,R> VisitOrCond) {
        this._VisitAndCond = VisitAndCond;
        this._VisitConditionClass = VisitConditionClass;
        this._VisitConditionPropertyRestriction = VisitConditionPropertyRestriction;
        this._VisitDatalogCondition = VisitDatalogCondition;
        this._VisitOrCond = VisitOrCond;
      }
      
      public R VisitAndCond(RailCNL.Condition x1,RailCNL.Condition x2) => _VisitAndCond(x1, x2);
      public R VisitConditionClass(RailCNL.Class x1) => _VisitConditionClass(x1);
      public R VisitConditionPropertyRestriction(RailCNL.PropertyRestriction x1) => _VisitConditionPropertyRestriction(x1);
      public R VisitDatalogCondition(RailCNL.Literal x1) => _VisitDatalogCondition(x1);
      public R VisitOrCond(RailCNL.Condition x1,RailCNL.Condition x2) => _VisitOrCond(x1, x2);
    }
    
    public static Condition FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Condition>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AndCond) && args.Length == 2)
            return new AndCond(Condition.FromExpression(args[0]), Condition.FromExpression(args[1]));
          if(fname == nameof(ConditionClass) && args.Length == 1)
            return new ConditionClass(Class.FromExpression(args[0]));
          if(fname == nameof(ConditionPropertyRestriction) && args.Length == 1)
            return new ConditionPropertyRestriction(PropertyRestriction.FromExpression(args[0]));
          if(fname == nameof(DatalogCondition) && args.Length == 1)
            return new DatalogCondition(Literal.FromExpression(args[0]));
          if(fname == nameof(OrCond) && args.Length == 2)
            return new OrCond(Condition.FromExpression(args[0]), Condition.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AndCond : Condition {
    public RailCNL.Condition x1 {get; set;}
    public RailCNL.Condition x2 {get; set;}
    public AndCond(RailCNL.Condition x1, RailCNL.Condition x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAndCond(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AndCond), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class ConditionClass : Condition {
    public RailCNL.Class x1 {get; set;}
    public ConditionClass(RailCNL.Class x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitConditionClass(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(ConditionClass), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class ConditionPropertyRestriction : Condition {
    public RailCNL.PropertyRestriction x1 {get; set;}
    public ConditionPropertyRestriction(RailCNL.PropertyRestriction x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitConditionPropertyRestriction(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(ConditionPropertyRestriction), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class DatalogCondition : Condition {
    public RailCNL.Literal x1 {get; set;}
    public DatalogCondition(RailCNL.Literal x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitDatalogCondition(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(DatalogCondition), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class OrCond : Condition {
    public RailCNL.Condition x1 {get; set;}
    public RailCNL.Condition x2 {get; set;}
    public OrCond(RailCNL.Condition x1, RailCNL.Condition x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitOrCond(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(OrCond), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class Conjunction : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitConj(RailCNL.Conjunction x1,RailCNL.Conjunction x2);
      R VisitEqLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitGtLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitGteLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitLtLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitLteLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitNegation(RailCNL.Literal x1);
      R VisitNeqLit(RailCNL.Term x1,RailCNL.Term x2);
      R VisitSimpleConj(RailCNL.Literal x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Conjunction,RailCNL.Conjunction,R> _VisitConj { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitEqLit { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitGtLit { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitGteLit { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitLtLit { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitLteLit { get; set; }
      private System.Func<RailCNL.Literal,R> _VisitNegation { get; set; }
      private System.Func<RailCNL.Term,RailCNL.Term,R> _VisitNeqLit { get; set; }
      private System.Func<RailCNL.Literal,R> _VisitSimpleConj { get; set; }
      public Visitor(System.Func<RailCNL.Conjunction,RailCNL.Conjunction,R> VisitConj, System.Func<RailCNL.Term,RailCNL.Term,R> VisitEqLit, System.Func<RailCNL.Term,RailCNL.Term,R> VisitGtLit, System.Func<RailCNL.Term,RailCNL.Term,R> VisitGteLit, System.Func<RailCNL.Term,RailCNL.Term,R> VisitLtLit, System.Func<RailCNL.Term,RailCNL.Term,R> VisitLteLit, System.Func<RailCNL.Literal,R> VisitNegation, System.Func<RailCNL.Term,RailCNL.Term,R> VisitNeqLit, System.Func<RailCNL.Literal,R> VisitSimpleConj) {
        this._VisitConj = VisitConj;
        this._VisitEqLit = VisitEqLit;
        this._VisitGtLit = VisitGtLit;
        this._VisitGteLit = VisitGteLit;
        this._VisitLtLit = VisitLtLit;
        this._VisitLteLit = VisitLteLit;
        this._VisitNegation = VisitNegation;
        this._VisitNeqLit = VisitNeqLit;
        this._VisitSimpleConj = VisitSimpleConj;
      }
      
      public R VisitConj(RailCNL.Conjunction x1,RailCNL.Conjunction x2) => _VisitConj(x1, x2);
      public R VisitEqLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitEqLit(x1, x2);
      public R VisitGtLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitGtLit(x1, x2);
      public R VisitGteLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitGteLit(x1, x2);
      public R VisitLtLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitLtLit(x1, x2);
      public R VisitLteLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitLteLit(x1, x2);
      public R VisitNegation(RailCNL.Literal x1) => _VisitNegation(x1);
      public R VisitNeqLit(RailCNL.Term x1,RailCNL.Term x2) => _VisitNeqLit(x1, x2);
      public R VisitSimpleConj(RailCNL.Literal x1) => _VisitSimpleConj(x1);
    }
    
    public static Conjunction FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Conjunction>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(Conj) && args.Length == 2)
            return new Conj(Conjunction.FromExpression(args[0]), Conjunction.FromExpression(args[1]));
          if(fname == nameof(EqLit) && args.Length == 2)
            return new EqLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(GtLit) && args.Length == 2)
            return new GtLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(GteLit) && args.Length == 2)
            return new GteLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(LtLit) && args.Length == 2)
            return new LtLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(LteLit) && args.Length == 2)
            return new LteLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(Negation) && args.Length == 1)
            return new Negation(Literal.FromExpression(args[0]));
          if(fname == nameof(NeqLit) && args.Length == 2)
            return new NeqLit(Term.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(SimpleConj) && args.Length == 1)
            return new SimpleConj(Literal.FromExpression(args[0]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class Conj : Conjunction {
    public RailCNL.Conjunction x1 {get; set;}
    public RailCNL.Conjunction x2 {get; set;}
    public Conj(RailCNL.Conjunction x1, RailCNL.Conjunction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitConj(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Conj), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class EqLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public EqLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitEqLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(EqLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class GtLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public GtLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitGtLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(GtLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class GteLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public GteLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitGteLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(GteLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class LtLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public LtLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLtLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(LtLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class LteLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public LteLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLteLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(LteLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class Negation : Conjunction {
    public RailCNL.Literal x1 {get; set;}
    public Negation(RailCNL.Literal x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitNegation(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Negation), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class NeqLit : Conjunction {
    public RailCNL.Term x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public NeqLit(RailCNL.Term x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitNeqLit(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(NeqLit), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class SimpleConj : Conjunction {
    public RailCNL.Literal x1 {get; set;}
    public SimpleConj(RailCNL.Literal x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitSimpleConj(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(SimpleConj), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public abstract class DirectionalObject : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAnyDirectionObject(RailCNL.Object x1);
      R VisitFacingSwitch();
      R VisitOppositeDirectionObject(RailCNL.Object x1);
      R VisitOppositeSearchDirecitonObject(RailCNL.Object x1);
      R VisitSameDirectionObject(RailCNL.Object x1);
      R VisitSearchDirectionObject(RailCNL.Object x1);
      R VisitTrailingSwitch();
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Object,R> _VisitAnyDirectionObject { get; set; }
      private System.Func<R> _VisitFacingSwitch { get; set; }
      private System.Func<RailCNL.Object,R> _VisitOppositeDirectionObject { get; set; }
      private System.Func<RailCNL.Object,R> _VisitOppositeSearchDirecitonObject { get; set; }
      private System.Func<RailCNL.Object,R> _VisitSameDirectionObject { get; set; }
      private System.Func<RailCNL.Object,R> _VisitSearchDirectionObject { get; set; }
      private System.Func<R> _VisitTrailingSwitch { get; set; }
      public Visitor(System.Func<RailCNL.Object,R> VisitAnyDirectionObject, System.Func<R> VisitFacingSwitch, System.Func<RailCNL.Object,R> VisitOppositeDirectionObject, System.Func<RailCNL.Object,R> VisitOppositeSearchDirecitonObject, System.Func<RailCNL.Object,R> VisitSameDirectionObject, System.Func<RailCNL.Object,R> VisitSearchDirectionObject, System.Func<R> VisitTrailingSwitch) {
        this._VisitAnyDirectionObject = VisitAnyDirectionObject;
        this._VisitFacingSwitch = VisitFacingSwitch;
        this._VisitOppositeDirectionObject = VisitOppositeDirectionObject;
        this._VisitOppositeSearchDirecitonObject = VisitOppositeSearchDirecitonObject;
        this._VisitSameDirectionObject = VisitSameDirectionObject;
        this._VisitSearchDirectionObject = VisitSearchDirectionObject;
        this._VisitTrailingSwitch = VisitTrailingSwitch;
      }
      
      public R VisitAnyDirectionObject(RailCNL.Object x1) => _VisitAnyDirectionObject(x1);
      public R VisitFacingSwitch() => _VisitFacingSwitch();
      public R VisitOppositeDirectionObject(RailCNL.Object x1) => _VisitOppositeDirectionObject(x1);
      public R VisitOppositeSearchDirecitonObject(RailCNL.Object x1) => _VisitOppositeSearchDirecitonObject(x1);
      public R VisitSameDirectionObject(RailCNL.Object x1) => _VisitSameDirectionObject(x1);
      public R VisitSearchDirectionObject(RailCNL.Object x1) => _VisitSearchDirectionObject(x1);
      public R VisitTrailingSwitch() => _VisitTrailingSwitch();
    }
    
    public static DirectionalObject FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<DirectionalObject>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AnyDirectionObject) && args.Length == 1)
            return new AnyDirectionObject(Object.FromExpression(args[0]));
          if(fname == nameof(FacingSwitch) && args.Length == 0)
            return new FacingSwitch();
          if(fname == nameof(OppositeDirectionObject) && args.Length == 1)
            return new OppositeDirectionObject(Object.FromExpression(args[0]));
          if(fname == nameof(OppositeSearchDirecitonObject) && args.Length == 1)
            return new OppositeSearchDirecitonObject(Object.FromExpression(args[0]));
          if(fname == nameof(SameDirectionObject) && args.Length == 1)
            return new SameDirectionObject(Object.FromExpression(args[0]));
          if(fname == nameof(SearchDirectionObject) && args.Length == 1)
            return new SearchDirectionObject(Object.FromExpression(args[0]));
          if(fname == nameof(TrailingSwitch) && args.Length == 0)
            return new TrailingSwitch();
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AnyDirectionObject : DirectionalObject {
    public RailCNL.Object x1 {get; set;}
    public AnyDirectionObject(RailCNL.Object x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAnyDirectionObject(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AnyDirectionObject), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class FacingSwitch : DirectionalObject {
    public FacingSwitch() {
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitFacingSwitch();
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(FacingSwitch), new PGF.Expression[]{});
    }
    
  }
  
  public class OppositeDirectionObject : DirectionalObject {
    public RailCNL.Object x1 {get; set;}
    public OppositeDirectionObject(RailCNL.Object x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitOppositeDirectionObject(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(OppositeDirectionObject), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class OppositeSearchDirecitonObject : DirectionalObject {
    public RailCNL.Object x1 {get; set;}
    public OppositeSearchDirecitonObject(RailCNL.Object x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitOppositeSearchDirecitonObject(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(OppositeSearchDirecitonObject), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class SameDirectionObject : DirectionalObject {
    public RailCNL.Object x1 {get; set;}
    public SameDirectionObject(RailCNL.Object x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitSameDirectionObject(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(SameDirectionObject), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class SearchDirectionObject : DirectionalObject {
    public RailCNL.Object x1 {get; set;}
    public SearchDirectionObject(RailCNL.Object x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitSearchDirectionObject(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(SearchDirectionObject), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class TrailingSwitch : DirectionalObject {
    public TrailingSwitch() {
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitTrailingSwitch();
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(TrailingSwitch), new PGF.Expression[]{});
    }
    
  }
  
  public abstract class Float : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
    }
    
    public class Visitor<R> : IVisitor<R> {
      public Visitor() {
      }
      
    }
    
    public static Float FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Float>() {
        fVisitApplication = (fname,args) =>  {
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public abstract class GoalObject : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAnyFound(RailCNL.DirectionalObject x1);
      R VisitFirstFound(RailCNL.DirectionalObject x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.DirectionalObject,R> _VisitAnyFound { get; set; }
      private System.Func<RailCNL.DirectionalObject,R> _VisitFirstFound { get; set; }
      public Visitor(System.Func<RailCNL.DirectionalObject,R> VisitAnyFound, System.Func<RailCNL.DirectionalObject,R> VisitFirstFound) {
        this._VisitAnyFound = VisitAnyFound;
        this._VisitFirstFound = VisitFirstFound;
      }
      
      public R VisitAnyFound(RailCNL.DirectionalObject x1) => _VisitAnyFound(x1);
      public R VisitFirstFound(RailCNL.DirectionalObject x1) => _VisitFirstFound(x1);
    }
    
    public static GoalObject FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<GoalObject>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AnyFound) && args.Length == 1)
            return new AnyFound(DirectionalObject.FromExpression(args[0]));
          if(fname == nameof(FirstFound) && args.Length == 1)
            return new FirstFound(DirectionalObject.FromExpression(args[0]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AnyFound : GoalObject {
    public RailCNL.DirectionalObject x1 {get; set;}
    public AnyFound(RailCNL.DirectionalObject x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAnyFound(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AnyFound), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class FirstFound : GoalObject {
    public RailCNL.DirectionalObject x1 {get; set;}
    public FirstFound(RailCNL.DirectionalObject x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitFirstFound(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(FirstFound), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public abstract class Int : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
    }
    
    public class Visitor<R> : IVisitor<R> {
      public Visitor() {
      }
      
    }
    
    public static Int FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Int>() {
        fVisitApplication = (fname,args) =>  {
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public abstract class Literal : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitLiteral0(RailCNL.Predicate x1);
      R VisitLiteral1(RailCNL.Predicate x1,RailCNL.Term x2);
      R VisitLiteral2(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3);
      R VisitLiteral3(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3,RailCNL.Term x4);
      R VisitLiteral4(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3,RailCNL.Term x4,RailCNL.Term x5);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Predicate,R> _VisitLiteral0 { get; set; }
      private System.Func<RailCNL.Predicate,RailCNL.Term,R> _VisitLiteral1 { get; set; }
      private System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,R> _VisitLiteral2 { get; set; }
      private System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,RailCNL.Term,R> _VisitLiteral3 { get; set; }
      private System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,RailCNL.Term,RailCNL.Term,R> _VisitLiteral4 { get; set; }
      public Visitor(System.Func<RailCNL.Predicate,R> VisitLiteral0, System.Func<RailCNL.Predicate,RailCNL.Term,R> VisitLiteral1, System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,R> VisitLiteral2, System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,RailCNL.Term,R> VisitLiteral3, System.Func<RailCNL.Predicate,RailCNL.Term,RailCNL.Term,RailCNL.Term,RailCNL.Term,R> VisitLiteral4) {
        this._VisitLiteral0 = VisitLiteral0;
        this._VisitLiteral1 = VisitLiteral1;
        this._VisitLiteral2 = VisitLiteral2;
        this._VisitLiteral3 = VisitLiteral3;
        this._VisitLiteral4 = VisitLiteral4;
      }
      
      public R VisitLiteral0(RailCNL.Predicate x1) => _VisitLiteral0(x1);
      public R VisitLiteral1(RailCNL.Predicate x1,RailCNL.Term x2) => _VisitLiteral1(x1, x2);
      public R VisitLiteral2(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3) => _VisitLiteral2(x1, x2, x3);
      public R VisitLiteral3(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3,RailCNL.Term x4) => _VisitLiteral3(x1, x2, x3, x4);
      public R VisitLiteral4(RailCNL.Predicate x1,RailCNL.Term x2,RailCNL.Term x3,RailCNL.Term x4,RailCNL.Term x5) => _VisitLiteral4(x1, x2, x3, x4, x5);
    }
    
    public static Literal FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Literal>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(Literal0) && args.Length == 1)
            return new Literal0(Predicate.FromExpression(args[0]));
          if(fname == nameof(Literal1) && args.Length == 2)
            return new Literal1(Predicate.FromExpression(args[0]), Term.FromExpression(args[1]));
          if(fname == nameof(Literal2) && args.Length == 3)
            return new Literal2(Predicate.FromExpression(args[0]), Term.FromExpression(args[1]), Term.FromExpression(args[2]));
          if(fname == nameof(Literal3) && args.Length == 4)
            return new Literal3(Predicate.FromExpression(args[0]), Term.FromExpression(args[1]), Term.FromExpression(args[2]), Term.FromExpression(args[3]));
          if(fname == nameof(Literal4) && args.Length == 5)
            return new Literal4(Predicate.FromExpression(args[0]), Term.FromExpression(args[1]), Term.FromExpression(args[2]), Term.FromExpression(args[3]), Term.FromExpression(args[4]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class Literal0 : Literal {
    public RailCNL.Predicate x1 {get; set;}
    public Literal0(RailCNL.Predicate x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLiteral0(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Literal0), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Literal1 : Literal {
    public RailCNL.Predicate x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public Literal1(RailCNL.Predicate x1, RailCNL.Term x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLiteral1(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Literal1), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class Literal2 : Literal {
    public RailCNL.Predicate x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public RailCNL.Term x3 {get; set;}
    public Literal2(RailCNL.Predicate x1, RailCNL.Term x2, RailCNL.Term x3) {
      this.x1 = x1;
      this.x2 = x2;
      this.x3 = x3;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLiteral2(x1, x2, x3);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Literal2), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression(), x3.ToExpression()});
    }
    
  }
  
  public class Literal3 : Literal {
    public RailCNL.Predicate x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public RailCNL.Term x3 {get; set;}
    public RailCNL.Term x4 {get; set;}
    public Literal3(RailCNL.Predicate x1, RailCNL.Term x2, RailCNL.Term x3, RailCNL.Term x4) {
      this.x1 = x1;
      this.x2 = x2;
      this.x3 = x3;
      this.x4 = x4;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLiteral3(x1, x2, x3, x4);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Literal3), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression(), x3.ToExpression(), x4.ToExpression()});
    }
    
  }
  
  public class Literal4 : Literal {
    public RailCNL.Predicate x1 {get; set;}
    public RailCNL.Term x2 {get; set;}
    public RailCNL.Term x3 {get; set;}
    public RailCNL.Term x4 {get; set;}
    public RailCNL.Term x5 {get; set;}
    public Literal4(RailCNL.Predicate x1, RailCNL.Term x2, RailCNL.Term x3, RailCNL.Term x4, RailCNL.Term x5) {
      this.x1 = x1;
      this.x2 = x2;
      this.x3 = x3;
      this.x4 = x4;
      this.x5 = x5;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLiteral4(x1, x2, x3, x4, x5);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Literal4), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression(), x3.ToExpression(), x4.ToExpression(), x5.ToExpression()});
    }
    
  }
  
  public abstract class Object : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitObjectClass(RailCNL.Class x1);
      R VisitObjectPropertyRestriction(RailCNL.Class x1,RailCNL.PropertyRestriction x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Class,R> _VisitObjectClass { get; set; }
      private System.Func<RailCNL.Class,RailCNL.PropertyRestriction,R> _VisitObjectPropertyRestriction { get; set; }
      public Visitor(System.Func<RailCNL.Class,R> VisitObjectClass, System.Func<RailCNL.Class,RailCNL.PropertyRestriction,R> VisitObjectPropertyRestriction) {
        this._VisitObjectClass = VisitObjectClass;
        this._VisitObjectPropertyRestriction = VisitObjectPropertyRestriction;
      }
      
      public R VisitObjectClass(RailCNL.Class x1) => _VisitObjectClass(x1);
      public R VisitObjectPropertyRestriction(RailCNL.Class x1,RailCNL.PropertyRestriction x2) => _VisitObjectPropertyRestriction(x1, x2);
    }
    
    public static Object FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Object>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(ObjectClass) && args.Length == 1)
            return new ObjectClass(Class.FromExpression(args[0]));
          if(fname == nameof(ObjectPropertyRestriction) && args.Length == 2)
            return new ObjectPropertyRestriction(Class.FromExpression(args[0]), PropertyRestriction.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class ObjectClass : Object {
    public RailCNL.Class x1 {get; set;}
    public ObjectClass(RailCNL.Class x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitObjectClass(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(ObjectClass), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class ObjectPropertyRestriction : Object {
    public RailCNL.Class x1 {get; set;}
    public RailCNL.PropertyRestriction x2 {get; set;}
    public ObjectPropertyRestriction(RailCNL.Class x1, RailCNL.PropertyRestriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitObjectPropertyRestriction(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(ObjectPropertyRestriction), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class PathCondition : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitPathContains(RailCNL.DirectionalObject x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.DirectionalObject,R> _VisitPathContains { get; set; }
      public Visitor(System.Func<RailCNL.DirectionalObject,R> VisitPathContains) {
        this._VisitPathContains = VisitPathContains;
      }
      
      public R VisitPathContains(RailCNL.DirectionalObject x1) => _VisitPathContains(x1);
    }
    
    public static PathCondition FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<PathCondition>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(PathContains) && args.Length == 1)
            return new PathContains(DirectionalObject.FromExpression(args[0]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class PathContains : PathCondition {
    public RailCNL.DirectionalObject x1 {get; set;}
    public PathContains(RailCNL.DirectionalObject x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitPathContains(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(PathContains), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public abstract class Predicate : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitStringPredicate(string x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<string,R> _VisitStringPredicate { get; set; }
      public Visitor(System.Func<string,R> VisitStringPredicate) {
        this._VisitStringPredicate = VisitStringPredicate;
      }
      
      public R VisitStringPredicate(string x1) => _VisitStringPredicate(x1);
    }
    
    public static Predicate FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Predicate>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(StringPredicate) && args.Length == 1)
            return new StringPredicate(((PGF.LiteralString)args[0]).Value);
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class StringPredicate : Predicate {
    public string x1 {get; set;}
    public StringPredicate(string x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitStringPredicate(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(StringPredicate), new PGF.Expression[]{new PGF.LiteralString(x1)});
    }
    
  }
  
  public abstract class Property : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitStringProperty(string x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<string,R> _VisitStringProperty { get; set; }
      public Visitor(System.Func<string,R> VisitStringProperty) {
        this._VisitStringProperty = VisitStringProperty;
      }
      
      public R VisitStringProperty(string x1) => _VisitStringProperty(x1);
    }
    
    public static Property FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Property>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(StringProperty) && args.Length == 1)
            return new StringProperty(((PGF.LiteralString)args[0]).Value);
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class StringProperty : Property {
    public string x1 {get; set;}
    public StringProperty(string x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitStringProperty(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(StringProperty), new PGF.Expression[]{new PGF.LiteralString(x1)});
    }
    
  }
  
  public abstract class PropertyRestriction : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAndPropRestr(RailCNL.PropertyRestriction x1,RailCNL.PropertyRestriction x2);
      R VisitMkPropertyRestriction(RailCNL.Property x1,RailCNL.Restriction x2);
      R VisitOrPropRestr(RailCNL.PropertyRestriction x1,RailCNL.PropertyRestriction x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.PropertyRestriction,RailCNL.PropertyRestriction,R> _VisitAndPropRestr { get; set; }
      private System.Func<RailCNL.Property,RailCNL.Restriction,R> _VisitMkPropertyRestriction { get; set; }
      private System.Func<RailCNL.PropertyRestriction,RailCNL.PropertyRestriction,R> _VisitOrPropRestr { get; set; }
      public Visitor(System.Func<RailCNL.PropertyRestriction,RailCNL.PropertyRestriction,R> VisitAndPropRestr, System.Func<RailCNL.Property,RailCNL.Restriction,R> VisitMkPropertyRestriction, System.Func<RailCNL.PropertyRestriction,RailCNL.PropertyRestriction,R> VisitOrPropRestr) {
        this._VisitAndPropRestr = VisitAndPropRestr;
        this._VisitMkPropertyRestriction = VisitMkPropertyRestriction;
        this._VisitOrPropRestr = VisitOrPropRestr;
      }
      
      public R VisitAndPropRestr(RailCNL.PropertyRestriction x1,RailCNL.PropertyRestriction x2) => _VisitAndPropRestr(x1, x2);
      public R VisitMkPropertyRestriction(RailCNL.Property x1,RailCNL.Restriction x2) => _VisitMkPropertyRestriction(x1, x2);
      public R VisitOrPropRestr(RailCNL.PropertyRestriction x1,RailCNL.PropertyRestriction x2) => _VisitOrPropRestr(x1, x2);
    }
    
    public static PropertyRestriction FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<PropertyRestriction>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AndPropRestr) && args.Length == 2)
            return new AndPropRestr(PropertyRestriction.FromExpression(args[0]), PropertyRestriction.FromExpression(args[1]));
          if(fname == nameof(MkPropertyRestriction) && args.Length == 2)
            return new MkPropertyRestriction(Property.FromExpression(args[0]), Restriction.FromExpression(args[1]));
          if(fname == nameof(OrPropRestr) && args.Length == 2)
            return new OrPropRestr(PropertyRestriction.FromExpression(args[0]), PropertyRestriction.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AndPropRestr : PropertyRestriction {
    public RailCNL.PropertyRestriction x1 {get; set;}
    public RailCNL.PropertyRestriction x2 {get; set;}
    public AndPropRestr(RailCNL.PropertyRestriction x1, RailCNL.PropertyRestriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAndPropRestr(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AndPropRestr), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class MkPropertyRestriction : PropertyRestriction {
    public RailCNL.Property x1 {get; set;}
    public RailCNL.Restriction x2 {get; set;}
    public MkPropertyRestriction(RailCNL.Property x1, RailCNL.Restriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitMkPropertyRestriction(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(MkPropertyRestriction), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class OrPropRestr : PropertyRestriction {
    public RailCNL.PropertyRestriction x1 {get; set;}
    public RailCNL.PropertyRestriction x2 {get; set;}
    public OrPropRestr(RailCNL.PropertyRestriction x1, RailCNL.PropertyRestriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitOrPropRestr(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(OrPropRestr), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class Restriction : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAndRestr(RailCNL.Restriction x1,RailCNL.Restriction x2);
      R VisitEq(RailCNL.Value x1);
      R VisitGt(RailCNL.Value x1);
      R VisitGte(RailCNL.Value x1);
      R VisitLt(RailCNL.Value x1);
      R VisitLte(RailCNL.Value x1);
      R VisitNeq(RailCNL.Value x1);
      R VisitOrRestr(RailCNL.Restriction x1,RailCNL.Restriction x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Restriction,RailCNL.Restriction,R> _VisitAndRestr { get; set; }
      private System.Func<RailCNL.Value,R> _VisitEq { get; set; }
      private System.Func<RailCNL.Value,R> _VisitGt { get; set; }
      private System.Func<RailCNL.Value,R> _VisitGte { get; set; }
      private System.Func<RailCNL.Value,R> _VisitLt { get; set; }
      private System.Func<RailCNL.Value,R> _VisitLte { get; set; }
      private System.Func<RailCNL.Value,R> _VisitNeq { get; set; }
      private System.Func<RailCNL.Restriction,RailCNL.Restriction,R> _VisitOrRestr { get; set; }
      public Visitor(System.Func<RailCNL.Restriction,RailCNL.Restriction,R> VisitAndRestr, System.Func<RailCNL.Value,R> VisitEq, System.Func<RailCNL.Value,R> VisitGt, System.Func<RailCNL.Value,R> VisitGte, System.Func<RailCNL.Value,R> VisitLt, System.Func<RailCNL.Value,R> VisitLte, System.Func<RailCNL.Value,R> VisitNeq, System.Func<RailCNL.Restriction,RailCNL.Restriction,R> VisitOrRestr) {
        this._VisitAndRestr = VisitAndRestr;
        this._VisitEq = VisitEq;
        this._VisitGt = VisitGt;
        this._VisitGte = VisitGte;
        this._VisitLt = VisitLt;
        this._VisitLte = VisitLte;
        this._VisitNeq = VisitNeq;
        this._VisitOrRestr = VisitOrRestr;
      }
      
      public R VisitAndRestr(RailCNL.Restriction x1,RailCNL.Restriction x2) => _VisitAndRestr(x1, x2);
      public R VisitEq(RailCNL.Value x1) => _VisitEq(x1);
      public R VisitGt(RailCNL.Value x1) => _VisitGt(x1);
      public R VisitGte(RailCNL.Value x1) => _VisitGte(x1);
      public R VisitLt(RailCNL.Value x1) => _VisitLt(x1);
      public R VisitLte(RailCNL.Value x1) => _VisitLte(x1);
      public R VisitNeq(RailCNL.Value x1) => _VisitNeq(x1);
      public R VisitOrRestr(RailCNL.Restriction x1,RailCNL.Restriction x2) => _VisitOrRestr(x1, x2);
    }
    
    public static Restriction FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Restriction>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AndRestr) && args.Length == 2)
            return new AndRestr(Restriction.FromExpression(args[0]), Restriction.FromExpression(args[1]));
          if(fname == nameof(Eq) && args.Length == 1)
            return new Eq(Value.FromExpression(args[0]));
          if(fname == nameof(Gt) && args.Length == 1)
            return new Gt(Value.FromExpression(args[0]));
          if(fname == nameof(Gte) && args.Length == 1)
            return new Gte(Value.FromExpression(args[0]));
          if(fname == nameof(Lt) && args.Length == 1)
            return new Lt(Value.FromExpression(args[0]));
          if(fname == nameof(Lte) && args.Length == 1)
            return new Lte(Value.FromExpression(args[0]));
          if(fname == nameof(Neq) && args.Length == 1)
            return new Neq(Value.FromExpression(args[0]));
          if(fname == nameof(OrRestr) && args.Length == 2)
            return new OrRestr(Restriction.FromExpression(args[0]), Restriction.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AndRestr : Restriction {
    public RailCNL.Restriction x1 {get; set;}
    public RailCNL.Restriction x2 {get; set;}
    public AndRestr(RailCNL.Restriction x1, RailCNL.Restriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAndRestr(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AndRestr), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class Eq : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Eq(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitEq(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Eq), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Gt : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Gt(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitGt(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Gt), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Gte : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Gte(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitGte(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Gte), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Lt : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Lt(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLt(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Lt), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Lte : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Lte(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitLte(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Lte), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class Neq : Restriction {
    public RailCNL.Value x1 {get; set;}
    public Neq(RailCNL.Value x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitNeq(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Neq), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class OrRestr : Restriction {
    public RailCNL.Restriction x1 {get; set;}
    public RailCNL.Restriction x2 {get; set;}
    public OrRestr(RailCNL.Restriction x1, RailCNL.Restriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitOrRestr(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(OrRestr), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class Rule : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitMkRule(RailCNL.Literal x1,RailCNL.Conjunction x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Literal,RailCNL.Conjunction,R> _VisitMkRule { get; set; }
      public Visitor(System.Func<RailCNL.Literal,RailCNL.Conjunction,R> VisitMkRule) {
        this._VisitMkRule = VisitMkRule;
      }
      
      public R VisitMkRule(RailCNL.Literal x1,RailCNL.Conjunction x2) => _VisitMkRule(x1, x2);
    }
    
    public static Rule FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Rule>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(MkRule) && args.Length == 2)
            return new MkRule(Literal.FromExpression(args[0]), Conjunction.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class MkRule : Rule {
    public RailCNL.Literal x1 {get; set;}
    public RailCNL.Conjunction x2 {get; set;}
    public MkRule(RailCNL.Literal x1, RailCNL.Conjunction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitMkRule(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(MkRule), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class Statement : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitAllPathsObligation(RailCNL.Subject x1,RailCNL.GoalObject x2,RailCNL.PathCondition x3);
      R VisitConstraint(RailCNL.Subject x1,RailCNL.Condition x2);
      R VisitDistanceObligation(RailCNL.Subject x1,RailCNL.GoalObject x2,RailCNL.Restriction x3);
      R VisitObligation(RailCNL.Subject x1,RailCNL.Condition x2);
      R VisitRecommendation(RailCNL.Subject x1,RailCNL.Condition x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Subject,RailCNL.GoalObject,RailCNL.PathCondition,R> _VisitAllPathsObligation { get; set; }
      private System.Func<RailCNL.Subject,RailCNL.Condition,R> _VisitConstraint { get; set; }
      private System.Func<RailCNL.Subject,RailCNL.GoalObject,RailCNL.Restriction,R> _VisitDistanceObligation { get; set; }
      private System.Func<RailCNL.Subject,RailCNL.Condition,R> _VisitObligation { get; set; }
      private System.Func<RailCNL.Subject,RailCNL.Condition,R> _VisitRecommendation { get; set; }
      public Visitor(System.Func<RailCNL.Subject,RailCNL.GoalObject,RailCNL.PathCondition,R> VisitAllPathsObligation, System.Func<RailCNL.Subject,RailCNL.Condition,R> VisitConstraint, System.Func<RailCNL.Subject,RailCNL.GoalObject,RailCNL.Restriction,R> VisitDistanceObligation, System.Func<RailCNL.Subject,RailCNL.Condition,R> VisitObligation, System.Func<RailCNL.Subject,RailCNL.Condition,R> VisitRecommendation) {
        this._VisitAllPathsObligation = VisitAllPathsObligation;
        this._VisitConstraint = VisitConstraint;
        this._VisitDistanceObligation = VisitDistanceObligation;
        this._VisitObligation = VisitObligation;
        this._VisitRecommendation = VisitRecommendation;
      }
      
      public R VisitAllPathsObligation(RailCNL.Subject x1,RailCNL.GoalObject x2,RailCNL.PathCondition x3) => _VisitAllPathsObligation(x1, x2, x3);
      public R VisitConstraint(RailCNL.Subject x1,RailCNL.Condition x2) => _VisitConstraint(x1, x2);
      public R VisitDistanceObligation(RailCNL.Subject x1,RailCNL.GoalObject x2,RailCNL.Restriction x3) => _VisitDistanceObligation(x1, x2, x3);
      public R VisitObligation(RailCNL.Subject x1,RailCNL.Condition x2) => _VisitObligation(x1, x2);
      public R VisitRecommendation(RailCNL.Subject x1,RailCNL.Condition x2) => _VisitRecommendation(x1, x2);
    }
    
    public static Statement FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Statement>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(AllPathsObligation) && args.Length == 3)
            return new AllPathsObligation(Subject.FromExpression(args[0]), GoalObject.FromExpression(args[1]), PathCondition.FromExpression(args[2]));
          if(fname == nameof(Constraint) && args.Length == 2)
            return new Constraint(Subject.FromExpression(args[0]), Condition.FromExpression(args[1]));
          if(fname == nameof(DistanceObligation) && args.Length == 3)
            return new DistanceObligation(Subject.FromExpression(args[0]), GoalObject.FromExpression(args[1]), Restriction.FromExpression(args[2]));
          if(fname == nameof(Obligation) && args.Length == 2)
            return new Obligation(Subject.FromExpression(args[0]), Condition.FromExpression(args[1]));
          if(fname == nameof(Recommendation) && args.Length == 2)
            return new Recommendation(Subject.FromExpression(args[0]), Condition.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class AllPathsObligation : Statement {
    public RailCNL.Subject x1 {get; set;}
    public RailCNL.GoalObject x2 {get; set;}
    public RailCNL.PathCondition x3 {get; set;}
    public AllPathsObligation(RailCNL.Subject x1, RailCNL.GoalObject x2, RailCNL.PathCondition x3) {
      this.x1 = x1;
      this.x2 = x2;
      this.x3 = x3;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitAllPathsObligation(x1, x2, x3);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(AllPathsObligation), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression(), x3.ToExpression()});
    }
    
  }
  
  public class Constraint : Statement {
    public RailCNL.Subject x1 {get; set;}
    public RailCNL.Condition x2 {get; set;}
    public Constraint(RailCNL.Subject x1, RailCNL.Condition x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitConstraint(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Constraint), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class DistanceObligation : Statement {
    public RailCNL.Subject x1 {get; set;}
    public RailCNL.GoalObject x2 {get; set;}
    public RailCNL.Restriction x3 {get; set;}
    public DistanceObligation(RailCNL.Subject x1, RailCNL.GoalObject x2, RailCNL.Restriction x3) {
      this.x1 = x1;
      this.x2 = x2;
      this.x3 = x3;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitDistanceObligation(x1, x2, x3);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(DistanceObligation), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression(), x3.ToExpression()});
    }
    
  }
  
  public class Obligation : Statement {
    public RailCNL.Subject x1 {get; set;}
    public RailCNL.Condition x2 {get; set;}
    public Obligation(RailCNL.Subject x1, RailCNL.Condition x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitObligation(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Obligation), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public class Recommendation : Statement {
    public RailCNL.Subject x1 {get; set;}
    public RailCNL.Condition x2 {get; set;}
    public Recommendation(RailCNL.Subject x1, RailCNL.Condition x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitRecommendation(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(Recommendation), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class String : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
    }
    
    public class Visitor<R> : IVisitor<R> {
      public Visitor() {
      }
      
    }
    
    public static String FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<String>() {
        fVisitApplication = (fname,args) =>  {
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public abstract class Subject : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitSubjectClass(RailCNL.Class x1);
      R VisitSubjectPropertyRestriction(RailCNL.Class x1,RailCNL.PropertyRestriction x2);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Class,R> _VisitSubjectClass { get; set; }
      private System.Func<RailCNL.Class,RailCNL.PropertyRestriction,R> _VisitSubjectPropertyRestriction { get; set; }
      public Visitor(System.Func<RailCNL.Class,R> VisitSubjectClass, System.Func<RailCNL.Class,RailCNL.PropertyRestriction,R> VisitSubjectPropertyRestriction) {
        this._VisitSubjectClass = VisitSubjectClass;
        this._VisitSubjectPropertyRestriction = VisitSubjectPropertyRestriction;
      }
      
      public R VisitSubjectClass(RailCNL.Class x1) => _VisitSubjectClass(x1);
      public R VisitSubjectPropertyRestriction(RailCNL.Class x1,RailCNL.PropertyRestriction x2) => _VisitSubjectPropertyRestriction(x1, x2);
    }
    
    public static Subject FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Subject>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(SubjectClass) && args.Length == 1)
            return new SubjectClass(Class.FromExpression(args[0]));
          if(fname == nameof(SubjectPropertyRestriction) && args.Length == 2)
            return new SubjectPropertyRestriction(Class.FromExpression(args[0]), PropertyRestriction.FromExpression(args[1]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class SubjectClass : Subject {
    public RailCNL.Class x1 {get; set;}
    public SubjectClass(RailCNL.Class x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitSubjectClass(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(SubjectClass), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
  public class SubjectPropertyRestriction : Subject {
    public RailCNL.Class x1 {get; set;}
    public RailCNL.PropertyRestriction x2 {get; set;}
    public SubjectPropertyRestriction(RailCNL.Class x1, RailCNL.PropertyRestriction x2) {
      this.x1 = x1;
      this.x2 = x2;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitSubjectPropertyRestriction(x1, x2);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(SubjectPropertyRestriction), new PGF.Expression[]{x1.ToExpression(), x2.ToExpression()});
    }
    
  }
  
  public abstract class Term : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitFloatTerm(double x1);
      R VisitIntTerm(int x1);
      R VisitStringTerm(string x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<double,R> _VisitFloatTerm { get; set; }
      private System.Func<int,R> _VisitIntTerm { get; set; }
      private System.Func<string,R> _VisitStringTerm { get; set; }
      public Visitor(System.Func<double,R> VisitFloatTerm, System.Func<int,R> VisitIntTerm, System.Func<string,R> VisitStringTerm) {
        this._VisitFloatTerm = VisitFloatTerm;
        this._VisitIntTerm = VisitIntTerm;
        this._VisitStringTerm = VisitStringTerm;
      }
      
      public R VisitFloatTerm(double x1) => _VisitFloatTerm(x1);
      public R VisitIntTerm(int x1) => _VisitIntTerm(x1);
      public R VisitStringTerm(string x1) => _VisitStringTerm(x1);
    }
    
    public static Term FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Term>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(FloatTerm) && args.Length == 1)
            return new FloatTerm(((PGF.LiteralFloat)args[0]).Value);
          if(fname == nameof(IntTerm) && args.Length == 1)
            return new IntTerm(((PGF.LiteralInt)args[0]).Value);
          if(fname == nameof(StringTerm) && args.Length == 1)
            return new StringTerm(((PGF.LiteralString)args[0]).Value);
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class FloatTerm : Term {
    public double x1 {get; set;}
    public FloatTerm(double x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitFloatTerm(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(FloatTerm), new PGF.Expression[]{new PGF.LiteralFloat(x1)});
    }
    
  }
  
  public class IntTerm : Term {
    public int x1 {get; set;}
    public IntTerm(int x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitIntTerm(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(IntTerm), new PGF.Expression[]{new PGF.LiteralInt(x1)});
    }
    
  }
  
  public class StringTerm : Term {
    public string x1 {get; set;}
    public StringTerm(string x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitStringTerm(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(StringTerm), new PGF.Expression[]{new PGF.LiteralString(x1)});
    }
    
  }
  
  public abstract class Value : Tree {
    public abstract R Accept<R>(IVisitor<R> visitor);
    public interface IVisitor<R> {
      R VisitMkValue(RailCNL.Term x1);
    }
    
    public class Visitor<R> : IVisitor<R> {
      private System.Func<RailCNL.Term,R> _VisitMkValue { get; set; }
      public Visitor(System.Func<RailCNL.Term,R> VisitMkValue) {
        this._VisitMkValue = VisitMkValue;
      }
      
      public R VisitMkValue(RailCNL.Term x1) => _VisitMkValue(x1);
    }
    
    public static Value FromExpression(PGF.Expression expr) {
      return expr.Accept(new PGF.Expression.Visitor<Value>() {
        fVisitApplication = (fname,args) =>  {
          if(fname == nameof(MkValue) && args.Length == 1)
            return new MkValue(Term.FromExpression(args[0]));
          throw new System.ArgumentOutOfRangeException();
        }
        
      });
      
    }
    
  }
  
  public class MkValue : Value {
    public RailCNL.Term x1 {get; set;}
    public MkValue(RailCNL.Term x1) {
      this.x1 = x1;
    }
    
    public override R Accept<R>(IVisitor<R> visitor) {
      return visitor.VisitMkValue(x1);
    }
    
    public override PGF.Expression ToExpression() {
      return new PGF.Application(nameof(MkValue), new PGF.Expression[]{x1.ToExpression()});
    }
    
  }
  
}

