using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace PGF
{
    public class Concrete
    {
		internal IntPtr Ptr {  get; private set; }
		public Grammar Grammar { get; private set;}

		private Concrete() {}

		internal static Concrete FromPtr(Grammar g, IntPtr ptr) {
			var concr = new Concrete ();
			concr.Grammar = g;
			concr.Ptr = ptr;
			return concr;
		}

		// FIXME WHat is this
		public string PrintName(string name) {
			throw new NotImplementedException ();
		}

		public string Name => Native.GetPermanentString(Native.pgf_concrete_name, Ptr);

		public override string ToString () => $"Concrete:{Name} of {Grammar.Name}";

		public IEnumerable<Expression> Parse(string str, string catName = null, double? heuristics = null, 
			Action Callback1 = null, Action Callback2 = null) {

			using (var parsePl = new NativeGU.PoolErr ())
			using (var catNameNativeStr = new Native.NativeString(catName ?? Grammar.StartCat))
			using (var sentenceNativeStr = new Native.NativeString(str))
			{
				var exprPl = NativeGU.gu_new_pool ();
				var callbackMap = Native.pgf_new_callbacks_map (this.Ptr, parsePl.Ptr);

				var iterator = Native.pgf_parse_with_heuristics (this.Ptr, catNameNativeStr.Ptr, 
					sentenceNativeStr.Ptr, heuristics ?? -1, callbackMap,
					parsePl.ErrPtr, parsePl.Ptr, exprPl);

				// Ptr is type PgfExprProb*

				if (parsePl.Exception) {
					throw new PGF.Exceptions.ParseErrorException ();
				} else {
					foreach (var ptr in NativeGU.Iterate(iterator, parsePl.Ptr)) {
						var exprProb = (Native.PgfExprProb)Marshal.PtrToStructure (ptr, typeof(Native.PgfExprProb));
						yield return Expression.FromPtr(exprProb.expr, IntPtr.Zero);
					}
				}
			}
		}

		public string Linearize(Expression e) {
			using (var tmpPool = new NativeGU.PoolErr ()) {
				var buf = NativeGU.gu_string_buf (tmpPool.Ptr);
				var out_ = NativeGU.gu_string_buf_out (buf);

				Native.pgf_linearize(Ptr, e.NativePtr, out_, tmpPool.ErrPtr);
				if (tmpPool.Exception) {
					throw new PGF.Exceptions.PGFException();
				} else {
					var cstr = NativeGU.gu_string_buf_freeze (buf, tmpPool.Ptr);
					return Native.NativeString.StringFromNativeUtf8 (cstr);
				}
			}
		}

		public string LinearizeAll(Expression e) {
			throw new NotImplementedException ();
		}

		public Bracket BracketedLinearize(Expression e) {
			using (var tmpPool = new NativeGU.PoolErr ()) {
				var cts = Native.pgf_lzr_concretize (Ptr, e.NativePtr, tmpPool.ErrPtr, tmpPool.Ptr);

				var ctree = IntPtr.Zero;
				NativeGU.gu_enum_next(cts, ref ctree, tmpPool.Ptr);

				if (ctree == IntPtr.Zero) {
					return null;
				}

				ctree = Native.pgf_lzr_wrap_linref (ctree, tmpPool.Ptr);

				var builder = new Bracket.BracketBuilder ();

				var mem = Marshal.AllocHGlobal(Marshal.SizeOf<Native.PgfLinFuncs>());
				Marshal.StructureToPtr<Native.PgfLinFuncs> (builder.LinFuncs, mem, false);

				Native.pgf_lzr_linearize (Ptr, ctree, 0, ref mem, tmpPool.Ptr);

				var b =  builder.Build ();

				Marshal.FreeHGlobal (mem);

				return b;
			}
		}
    }
}
