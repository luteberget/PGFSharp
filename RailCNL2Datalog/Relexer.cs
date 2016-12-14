using System;

namespace RailCNL2Datalog
{
	public class Relexer
	{
		public static string Input2GF(string input) {
			// A single sentence.
			var gf = input.Trim();

			// optionally starting with a
			// upper case letter (removed for GF input)
			input = input[0].ToString().ToLower() + input.Substring(1);

			// should end in a period.
			if (input [input.Length - 1] != '.') {
				throw new LexerException ("Sentence should end with a period.");
			}
			input = input.Substring(0, input.Length - 1);

			return input;
		}

		public static string GF2Input(string gf) {
			// Should be a statement (single sentence).

			// We just need to upper case the first character,
			var output = gf;
			output = output [0].ToString ().ToUpper () + output.Substring (1);

			//  and add a period.
			output += ".";


			return output;
		}
		
		private Relexer ()
		{
		}
	}
}

