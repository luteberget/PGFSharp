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

		private static string CategoryToCSharpDatatype (string namespc, string name)
		{
			// Special cases for built-in types
			// FIXME: maybe this is not needed -- the C# data types have accidentaly the same names?
			if (name == "Int")
				return "int";
			if (name == "Float")
				return "double";
			if (name == "String")
				return "string";
			if (name == VISITOR_PARAM)
				return name;

			return $"{namespc}.{name}";
		}

		private static IEnumerable<string> VariableNames ()
		{
			int i = 1;
			while (true) {
				yield return $"x{i++}";
			}
		}

		private static string ArgList (string namespc, IEnumerable<string> types)
		{
			return String.Join (",", types.Zip (VariableNames (), (t, v) => $"{CategoryToCSharpDatatype(namespc, t)} {v}"));
		}

		private static string TypeList (string namespc, IEnumerable<string> types) =>
		String.Join(",", types.Select(s => CategoryToCSharpDatatype(namespc, s)));

		private static string VISITOR_PARAM = "R";

		public static string ToString (Grammar grammar)
		{
			var b = new StringBuilder ();
			Action<string> print = s => b.AppendLine (s);

			using (var namespc = new Bracketed (print, $"namespace {grammar.Name}")) { 

				foreach (var cat in grammar.Categories) {
					using (var catClass = new Bracketed (namespc.Print, $"public abstract {cat.Name} : Tree")) { 
						catClass.Print ("public abstract R Accept<R>(IVisitor<R> visitor);");

						using (var visitorInterface = new Bracketed (catClass.Print, "public interface IVisitor<R>")) {
							foreach (var constr in cat.Constructors) {
								visitorInterface.Print ($"R Visit{constr.Name}({ArgList(grammar.Name, constr.ArgumentTypes)});");
							}
						}

						using (var visitorClass = new Bracketed (catClass.Print, "public class Visitor<R> : IVisitor<R>")) {
							var constrNames = cat.Constructors.Select (c => c.Name);
							var argLists = cat.Constructors.Select (c => ArgList (grammar.Name, c.ArgumentTypes));
							var varLists = cat.Constructors.Select (c => String.Join(", ", VariableNames ().Take (c.ArgumentTypes.Count ())));
							var lambdaTypes = cat.Constructors.Select (c => $"Func<{TypeList(grammar.Name, c.ArgumentTypes.Concat(new[]{VISITOR_PARAM}))}>");
							var funcFields = constrNames.Zip (lambdaTypes,
								                 (name, type) => $"private {type} _Visit{name} {{ get; set; }}");
							var constructorArgs = String.Join (", ", constrNames.Zip (lambdaTypes,
								                      (name, type) => $"{type} Visit{name}"));

							var constructorAssignments = constrNames.Select (c => $"this._Visit{c} = Visit{c};");
							var interfaceImplementationDecl = constrNames.Zip (argLists, 
								                                   (name, args) => $"public R Visit{name}({args})");
							var interfaceImplementationBody = constrNames.Zip (varLists,
								                                   (name, vars) => $"_Visit{name}({vars});");
							var interfaceImplementaion = interfaceImplementationDecl.Zip (interfaceImplementationBody,
								                               (decl, body) => $"{decl} => {body}");

							foreach (var s in funcFields)
								visitorClass.Print (s);

							using (var visitorConstructor = new Bracketed (visitorClass.Print, $"public Visitor({constructorArgs})")) { 
								foreach (var assign in constructorAssignments)
									visitorConstructor.Print (assign);
							}

							foreach (var impl in interfaceImplementaion)
								visitorClass.Print (impl);
						}
					}

					foreach (var constr in cat.Constructors) {

						using (var constrClass = new Bracketed (namespc.Print, $"public class {constr.Name} : {cat.Name}")) { 

							var fields = constr.ArgumentTypes.Zip(VariableNames(), (type, name) => $"public {type} {name} {{get; set;}}");
							var constructorArgs = String.Join(", ", constr.ArgumentTypes.Zip(VariableNames(), (type, name) => $"{type} {name}"));
							var vars = VariableNames().Take(constr.ArgumentTypes.Count());
							var varList = String.Join(", ", vars);
							var constructorAssignments = vars.Select(name => $"this.{name} = {name};");

							foreach(var f in fields) constrClass.Print(f);

							using(var constrConstructor = new Bracketed(constrClass.Print, $"public {constr.Name}({constructorArgs})")) {
								foreach(var a in constructorAssignments) constrConstructor.Print(a);
							}

							using(var visitorAccept = new Bracketed(constrClass.Print, "public override R Accept<R>(IVisitor<R> visitor)")) {
								visitorAccept.Print($"visitor.Visit{constr.Name}({varList});");
							}
						}
					}
				}
			}
			return b.ToString ();
		}

		public class Bracketed : IDisposable
		{
			Action<string> printer;

			public Bracketed (Action<string> printer, string something)
			{
				this.printer = printer;
				printer (something + " {");
			}

			public void Print (string s) => printer("  " + s);

			public void Dispose ()
			{
				printer ("}");
				printer ("");
			}
		}
	}
}

