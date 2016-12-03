using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace PGF
{
	public class Application : Expression
	{
		public override R Accept<R> (IVisitor<R> visitor)
		{
			var args = new List<Expression> ();
			var expr = this;
			while (expr.Function is Application) {
				args.Add (expr.Argument);
				expr = expr.Function as Application;
			}

			if (!(expr.Function is Function))
				throw new ArgumentException ();

			args.Reverse ();
			return visitor.VisitApplication ((expr.Function as Function).Name, args.ToArray());
		}
		
		[StructLayout(LayoutKind.Sequential)]
		public struct PgfExprApp {
			public IntPtr Function;
			public IntPtr Argument;
		}

		private PgfExprApp Data => Marshal.PtrToStructure<PgfExprApp>(DataPtr);

		public Expression Function => Expression.FromPtr(Data.Function, _pool);
		public Expression Argument => Expression.FromPtr(Data.Argument, _pool);

		internal Application(IntPtr ptr, IntPtr pool) : base(ptr, pool) {	}
		public Application (string fname, IEnumerable<Expression> args)
		{
			_pool = NativeGU.gu_new_pool ();
			MkStringVariant((byte)PgfExprTag.PGF_EXPR_FUN, fname, ref _expr);
			foreach (var arg in args) {
				var fun = _expr;
				var exprApp = NativeGU.gu_alloc_variant((byte)PgfExprTag.PGF_EXPR_APP,
					(UIntPtr)Marshal.SizeOf<PgfExprApp>(), UIntPtr.Zero, ref _expr, _pool);

				Native.EditStruct<PgfExprApp> (exprApp, (ref PgfExprApp app) => {
					app.Function = fun;
					app.Argument = arg.NativePtr;
				});
			}


		}
	}
}

