using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace PGF
{
	public class ApplicationExpression : Expression
	{
		public override R Accept<R> (IVisitor<R> visitor)
		{
			var args = new List<Expression> ();
			var expr = this;
			while (expr.Function is ApplicationExpression) {
				args.Add (expr.Argument);
				expr = expr.Function as ApplicationExpression;
			}
			args.Add (expr.Argument);
			if (!(expr.Function is FunctionExpression))
				throw new ArgumentException ();

			args.Reverse ();
			return visitor.VisitApplication ((expr.Function as FunctionExpression).Name, args.ToArray());
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct PgfExprApp {
			public IntPtr Function;
			public IntPtr Argument;
		}

		private PgfExprApp Data => Marshal.PtrToStructure<PgfExprApp>(DataPtr);

		public Expression Function => Expression.FromPtr(Data.Function, _pool);
		public Expression Argument => Expression.FromPtr(Data.Argument, _pool);

		internal ApplicationExpression(IntPtr ptr, NativeGU.NativeMemoryPool pool) : base(ptr, pool) {	}
		public ApplicationExpression (string fname, IEnumerable<Expression> args)
		{
			_pool = new NativeGU.NativeMemoryPool();
			MkStringVariant((byte)PgfExprTag.PGF_EXPR_FUN, fname, ref _ptr);
			foreach (var arg in args) {
				var fun = _ptr;
				var exprApp = NativeGU.gu_alloc_variant((byte)PgfExprTag.PGF_EXPR_APP,
					(UIntPtr)Marshal.SizeOf<PgfExprApp>(), UIntPtr.Zero, ref _ptr, _pool.Ptr);

				Native.EditStruct<PgfExprApp> (exprApp, (ref PgfExprApp app) => {
					app.Function = fun;
					app.Argument = arg.Ptr;
				});
			}


		}
	}
}

