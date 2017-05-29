using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ExportGFTypes
{
    public class CodeGenerator
    {
        public struct Grammar
        {
            public string Name;
            public List<Category> Categories;
        }

        public struct Category
        {
            public string Name;
            public List<Constructor> Constructors;
        }

        public struct Constructor
        {
            public string Name;
            public List<string> ArgumentTypes;
        }

        //private const string VisitorTypeParam = "R";
        private static string VISITOR_PARAM = "R";

        private static readonly Dictionary<string, string> _csharpTypeMap = new Dictionary<string, string> {
            {"Int","int"},
            {"Float","double"},
            {"String","string"},
            {VISITOR_PARAM, VISITOR_PARAM}
        };

        private static string CategoryToCSharpDatatype(string namespc, string name)
        {
            if (IsBuiltinType(name)) return _csharpTypeMap[name];
            return $"{namespc}.{name}";
        }

        private static bool IsBuiltinType(string name) => _csharpTypeMap.ContainsKey(name);

        private static IEnumerable<string> VariableNames()
        {
            int i = 1;
            while (true)
            {
                yield return $"x{i++}";
            }
        }

        private static string ArgList(string namespc, IEnumerable<string> types)
        {
            return String.Join(",", types.Zip(VariableNames(), (t, v) => $"{CategoryToCSharpDatatype(namespc, t)} {v}"));
        }

        private static string TypeList(string namespc, IEnumerable<string> types) =>
        String.Join(",", types.Select(s => CategoryToCSharpDatatype(namespc, s)));


        public static string ToString(Grammar grammar)
        {
            var b = new StringBuilder();
            Action<string> print = s => b.AppendLine(s);

            using (var tree = new Bracketed(print, "public abstract class Tree"))
            {
                tree.Print("public abstract PGF.Expression ToExpression();");
            }

            using (var namespc = new Bracketed(print, $"namespace {grammar.Name}"))
            {
                Func<string, string> typeName = n => CategoryToCSharpDatatype(grammar.Name, n);
                foreach (var cat in grammar.Categories)
                {

                    // Abstract class (category) 
                    using (var catClass = new Bracketed(namespc.Print, $"public abstract class {cat.Name} : Tree"))
                    {
                        catClass.Print("public abstract R Accept<R>(IVisitor<R> visitor);");

                        using (var visitorInterface = new Bracketed(catClass.Print, "public interface IVisitor<R>"))
                        {
                            foreach (var constr in cat.Constructors)
                            {
                                visitorInterface.Print($"R Visit{constr.Name}({ArgList(grammar.Name, constr.ArgumentTypes)});");
                            }
                        }

                        using (var visitorClass = new Bracketed(catClass.Print, "public class Visitor<R> : IVisitor<R>"))
                        {
                            var constrNames = cat.Constructors.Select(c => c.Name);
                            var argLists = cat.Constructors.Select(c => ArgList(grammar.Name, c.ArgumentTypes));
                            var varLists = cat.Constructors.Select(c => String.Join(", ", VariableNames().Take(c.ArgumentTypes.Count())));
                            var lambdaTypes = cat.Constructors.Select(c => $"System.Func<{TypeList(grammar.Name, c.ArgumentTypes.Concat(new[] { VISITOR_PARAM }))}>");
                            var funcFields = constrNames.Zip(lambdaTypes,
                                                 (name, type) => $"private {type} _Visit{name} {{ get; set; }}");
                            var constructorArgs = String.Join(", ", constrNames.Zip(lambdaTypes,
                                                      (name, type) => $"{type} Visit{name}"));

                            var constructorAssignments = constrNames.Select(c => $"this._Visit{c} = Visit{c};");
                            var interfaceImplementationDecl = constrNames.Zip(argLists,
                                                                   (name, args) => $"public R Visit{name}({args})");
                            var interfaceImplementationBody = constrNames.Zip(varLists,
                                                                   (name, vars) => $"_Visit{name}({vars});");
                            var interfaceImplementaion = interfaceImplementationDecl.Zip(interfaceImplementationBody,
                                                               (decl, body) => $"{decl} => {body}");

                            foreach (var s in funcFields)
                                visitorClass.Print(s);

                            using (var visitorConstructor = new Bracketed(visitorClass.Print, $"public Visitor({constructorArgs})"))
                            {
                                foreach (var assign in constructorAssignments)
                                    visitorConstructor.Print(assign);
                            }

                            foreach (var impl in interfaceImplementaion)
                                visitorClass.Print(impl);



                        }

                        // FromExpression
                        using (var fromExpr = new Bracketed(catClass.Print, $"public static {cat.Name} FromExpression(PGF.Expression expr)"))
                        {
                            //fromExpr.Print($"var visitor = new Expression.Visitor<{cat.Name}>();");
                            using (var visitor = new Bracketed(fromExpr.Print, $"return expr.Accept(new PGF.Expression.Visitor<{cat.Name}>()", ");"))
                            {
                                using (var visitApp = new Bracketed(visitor.Print, $"fVisitApplication = (fname,args) => "))
                                {
                                    foreach (var constr in cat.Constructors)
                                    {
                                        visitApp.Print($"if(fname == nameof({constr.Name}) && args.Length == {constr.ArgumentTypes.Count()})");

                                        var args = constr.ArgumentTypes.Select((t, i) =>
                                        {
                                            if (IsBuiltinType(t)) return $"((PGF.Literal{t})args[{i}]).Value";
                                            return $"{t}.FromExpression(args[{i}])";
                                        });

                                        visitApp.Print($"  return new {constr.Name}({String.Join(", ", args)});");

                                    }
                                    visitApp.Print("throw new System.ArgumentOutOfRangeException();");
                                }
                            }
                        }
                    }


                    foreach (var constr in cat.Constructors)
                    {

                        // Concrete class (category constructor)
                        using (var constrClass = new Bracketed(namespc.Print, $"public class {constr.Name} : {cat.Name}"))
                        {

                            var fields = constr.ArgumentTypes.Zip(VariableNames(), (type, name) => $"public {typeName(type)} {name} {{get; set;}}");
                            var constructorArgs = String.Join(", ", constr.ArgumentTypes.Zip(VariableNames(), (type, name) => $"{typeName(type)} {name}"));
                            var vars = VariableNames().Take(constr.ArgumentTypes.Count());
                            var varList = String.Join(", ", vars);
                            var constructorAssignments = vars.Select(name => $"this.{name} = {name};");

                            // Fields (for constructor arguments)
                            foreach (var f in fields) constrClass.Print(f);

                            // Constructor
                            using (var constrConstructor = new Bracketed(constrClass.Print, $"public {constr.Name}({constructorArgs})"))
                            {
                                foreach (var a in constructorAssignments) constrConstructor.Print(a);
                            }

                            // Visitor
                            using (var visitorAccept = new Bracketed(constrClass.Print, "public override R Accept<R>(IVisitor<R> visitor)"))
                            {
                                visitorAccept.Print($"return visitor.Visit{constr.Name}({varList});");
                            }

                            // FromExpression
                            using (var fromExpr = new Bracketed(constrClass.Print, "public override PGF.Expression ToExpression()"))
                            {
                                var args = constr.ArgumentTypes.Zip(VariableNames(), (type, name) =>
                                {
                                    if (IsBuiltinType(type)) return $"new PGF.Literal{type}({name})";
                                    return $"{name}.ToExpression()";
                                });
                                var argsArray = $"new PGF.Expression[]{{{String.Join(", ", args)}}}";
                                fromExpr.Print($"return new PGF.Application(nameof({constr.Name}), {argsArray});");
                            }
                        }
                    }
                }
            }
            return b.ToString();
        }

        public class Bracketed : IDisposable
        {
            Action<string> printer;
            string after;
            public Bracketed(Action<string> printer, string something, string after = null)
            {
                this.printer = printer;
                this.after = after;
                printer(something + " {");
            }

            public void Print(string s) => printer("  " + s);

            public void Dispose()
            {
                printer("}" + (after ?? ""));
                printer("");
            }
        }
    }
}

