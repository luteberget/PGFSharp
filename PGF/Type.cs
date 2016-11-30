using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public class Type
    {
		private Grammar Grammar;
		private IntPtr _type;

		private IntPtr _pool = IntPtr.Zero; // FIXME: Initialized to null? See Python wrapper: PGF_functionType

		private Type(Grammar grammar, IntPtr ptr) {
			this.Grammar = grammar;
			this._type = ptr;
		}

		private Type(IntPtr type, IntPtr pool) {
		}

		internal static Type FromPtr(Grammar grammar, IntPtr type) {
			return new Type (grammar, type);
		}

		internal static Type FromPtrs(IntPtr type, IntPtr pool) {
			return new Type (type, pool);
		}
    }
}
