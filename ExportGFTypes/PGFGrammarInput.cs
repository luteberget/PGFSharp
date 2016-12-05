using System;
using System.Linq;

namespace ExportGFTypes
{
	// Convert PGF grammar to CodeGenerator grammar.
	public class PGFGrammarInput
	{
		public static CodeGenerator.Grammar Convert(PGF.Grammar grammar) {
			return new CodeGenerator.Grammar {
				Name = grammar.Name,
				Categories = grammar.Categories.Select(catName => 
					new CodeGenerator.Category {
						Name = catName,
						Constructors = grammar.FunctionByCategory(catName).Select(funName => 
							new CodeGenerator.Constructor {
								Name = funName,
								ArgumentTypes = Enumerable.Empty<string>().ToList(),// FIXME read the argument types from grammar.
							}).ToList()
					}).ToList()
			};
		}
	}
}
