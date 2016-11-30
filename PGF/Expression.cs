using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    // TODO
    // UNFINISHED DO NOT USE
    public class Expression
    {
        private IntPtr _expr = IntPtr.Zero;
        
		// The master pointer is used in the Python wrapper, 
		// which is written in C and therefore needs to help Python 
		// with reference counting. So we don't need it here.
		// I think.
		//private IntPtr _master = IntPtr.Zero;
        
		private IntPtr _pool = IntPtr.Zero;

		public IntPtr NativePtr => _expr;

		private Expression()
        {
        }

		public static Expression FromPtr(IntPtr expr, IntPtr pool) {
			return new Expression () {
				_expr = expr,
				_pool = pool,
			};
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

        ~Expression()
        {
            if(_pool != IntPtr.Zero)
            {
                NativeGU.gu_pool_free(_pool);
                _pool = IntPtr.Zero;
            }
        }
    }
}
