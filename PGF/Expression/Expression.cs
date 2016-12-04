using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
	// Expression types work list
	//   PGF_EXPR_IMPL_ARG = ?
	//   PGF_EXPR_TYPED = ?
	//   PGF_EXPR_VAR = variable?
	// X PGF_EXPR_FUN = function (name) ?
	// X PGF_EXPR_LIT = literal
	// X PGF_EXPR_META = Meta Variable, has id number.
	// X PGF_EXPR_APP = function application
	//   PGF_AXPR_ABS = function abstraction with 
	//      (a) bind type = explicit/implicit (b) variable 
	//      (c) body (d) arguments (seems to be empty list in python wrapper?)

	public class UnsupportedExpression : Expression {
		internal UnsupportedExpression(IntPtr expr, IntPtr pool) : base(expr, pool) {}
		public override R Accept<R> (IVisitor<R> visitor)
		{
			throw new NotImplementedException ();
		}
	}

    public abstract class Expression
    {
		protected IntPtr DataPtr => NativeGU.gu_variant_open(_expr).Data; // PgfExprLit* 
		protected PgfExprTag Tag => (PgfExprTag) NativeGU.gu_variant_open(_expr).Tag;

		protected IntPtr MkStringVariant(byte tag, string s, ref IntPtr out_) {
			var size = Encoding.UTF8.GetByteCount(s);
			IntPtr slitPtr = NativeGU.gu_alloc_variant (tag,
				(UIntPtr)(size+1), UIntPtr.Zero, ref out_, _pool);
			Native.NativeString.CopyToPreallocated(s, slitPtr);
			return slitPtr;
		}

		public enum PgfExprTag {
			PGF_EXPR_ABS,
			PGF_EXPR_APP,
			PGF_EXPR_LIT,
			PGF_EXPR_META,
			PGF_EXPR_FUN,
			PGF_EXPR_VAR,
			PGF_EXPR_TYPED,
			PGF_EXPR_IMPL_ARG,
			PGF_EXPR_NUM_TAGS // not used
		};

		public interface IVisitor<R> {
			R VisitLiteralInt (int value);
			R VisitLiteralFlt (double value);
			R VisitLiteralStr (string value);
			R VisitApplication (string fname, Expression[] args);

			//R VisitMetaVariable (int id); Dont' care about this for now...

			// Remove this, Function objects use VisitApplication with empty args instead.
			//R VisitFunction (string fname); // Will this be used?
		}

		public class Visitor<R> : IVisitor<R> {

			public Func<int,R> fVisitLiteralInt { get; set; } = null;
			public R VisitLiteralInt (int x1) => fVisitLiteralInt(x1);
			public Func<double,R> fVisitLiteralFlt { get; set; } = null;
			public R VisitLiteralFlt (double x1) => fVisitLiteralFlt(x1);
			public Func<string,R> fVisitLiteralStr { get; set; } = null;
			public R VisitLiteralStr (string x1) => fVisitLiteralStr(x1);
			public Func<string, Expression[] ,R> fVisitApplication { get; set; } = null;
			public R VisitApplication (string x1, Expression[] x2) => fVisitApplication(x1, x2);
		}

		public abstract R Accept<R> (IVisitor<R> visitor);
		
        protected IntPtr _expr = IntPtr.Zero;
        
		// The master pointer is used in the Python wrapper, 
		// which is written in C and therefore needs to help Python 
		// with reference counting. So we don't need it here.
		// I think.
		//private IntPtr _master = IntPtr.Zero;
        
		protected IntPtr _pool = IntPtr.Zero;

		public IntPtr NativePtr { get {return _expr; } }


		protected Expression() {}

		protected Expression(IntPtr ptr, IntPtr pool)
        {
			_expr = ptr; _pool = pool;
        }

		// Factories
		private static Dictionary<PgfExprTag, Func<IntPtr, IntPtr, Expression>> factories = 
			new Dictionary<PgfExprTag, Func<IntPtr, IntPtr, Expression>>{

			{ PgfExprTag.PGF_EXPR_LIT, (e, p) => new Literal (e, p) },
			{ PgfExprTag.PGF_EXPR_APP, (e, p) => new Application (e, p) },
			{ PgfExprTag.PGF_EXPR_FUN, (e, p) => new Function (e, p) },
			{ PgfExprTag.PGF_EXPR_META, (e, p) => new MetaVariable (e, p) }
		};

		public static Expression FromPtr(IntPtr expr, IntPtr pool) {
			var Tag = (PgfExprTag) NativeGU.gu_variant_open(expr).Tag;
			if (factories.ContainsKey (Tag)) {
				return factories [Tag] (expr, pool);
			} else
				return new UnsupportedExpression (expr, pool);
		}

        public override string ToString()
        {
			using (var pool = new NativeGU.PoolErr ()) {
				var sbuf = NativeGU.gu_string_buf (pool.Ptr);
				var output = NativeGU.gu_string_buf_out (sbuf);

				Native.pgf_print_expr (_expr, IntPtr.Zero, 0, output, pool.ErrPtr);

				var strPtr = NativeGU.gu_string_buf_freeze (sbuf, pool.Ptr);
				var str = Native.NativeString.StringFromNativeUtf8 (strPtr);
				return str;
			}
        }


		/*
        ~Expression()
        {
            if(_pool != IntPtr.Zero)
            {
                NativeGU.gu_pool_free(_pool);
                _pool = IntPtr.Zero;
            }
        }*/
    }
}
