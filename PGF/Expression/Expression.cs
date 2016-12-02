using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public abstract class Expression
    {
		public enum PgfExprTag {
			PGF_EXPR_ABS,
			PGF_EXPR_APP,
			PGF_EXPR_LIT,
			PGF_EXPR_META,
			PGF_EXPR_FUN,
			PGF_EXPR_VAR,
			PGF_EXPR_TYPED,
			PGF_EXPR_IMPL_ARG,
			PGF_EXPR_NUM_TAGS
		};



		
		public interface IVisitor<R> {
			R VisitLiteral (object value);
			R VisitMetaVariable (int id);
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

		protected Expression()
        {
        }


		public static Expression FromPtr(IntPtr expr, IntPtr pool) {
			return null;
			/*return new Expression () {
				
				_expr = expr,
				_pool = pool,
			};*/
		}

        public override string ToString()
        {
			using (var pool = new NativeGU.PoolErr ()) {
				var sbuf = NativeGU.gu_string_buf (pool.Ptr);
				var output = NativeGU.gu_string_buf_out (sbuf);

				Native.pgf_print_expr (_expr, IntPtr.Zero, 0, output, pool.ErrPtr);

				var strPtr = NativeGU.gu_string_buf_freeze (sbuf, pool.Ptr);
				var str = Marshal.PtrToStringAnsi (strPtr);
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
        }
        */
    }
}
