using NUnit.Framework;
using System;

namespace ExportGFTypes.NUnit
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void TestCase ()
		{
			/*
			var query = new Query.Even (new Query.Number (15));
			var queryString = query.ToExpressionString ();
			//var expr = PGF.Grammar.
			*/
		}

		[Test]
		public void CodeGeneratorTest() {

			var abs = new CodeGenerator.Grammar {
				Name = "Query",
				Categories = new System.Collections.Generic.List<CodeGenerator.Category> {
					new CodeGenerator.Category {
						Name = "Answer",

						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Yes",
								ArgumentTypes = new System.Collections.Generic.List<string> { }
							},
							new CodeGenerator.Constructor {
								Name = "No",
								ArgumentTypes = new System.Collections.Generic.List<string> { }
							},
						},
					},
					new CodeGenerator.Category {
						Name = "Object",
						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Number",
								ArgumentTypes = new System.Collections.Generic.List<string> {
									"Int"
								},
							}
						}
					},
					new CodeGenerator.Category {
						Name = "Question",
						Constructors = new System.Collections.Generic.List<CodeGenerator.Constructor> {
							new CodeGenerator.Constructor {
								Name = "Even",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
							new CodeGenerator.Constructor {
								Name = "Odd",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
							new CodeGenerator.Constructor {
								Name = "Prime",
								ArgumentTypes = new System.Collections.Generic.List<string>{ "Object" },
							},
						}
					}
				}
			};

			var output = CodeGenerator.ToString (abs);
			
		}
	}
}

