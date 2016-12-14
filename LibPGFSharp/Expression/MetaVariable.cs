using System;
using System.Runtime.InteropServices;

namespace PGF
{
	public class MetaVariable : Expression {
		// initMeta
		public MetaVariable() {
			_pool = NativeGU.gu_new_pool ();
			IntPtr exprMetaPtr = NativeGU.gu_alloc_variant ((byte)PgfExprTag.PGF_EXPR_META,
				(UIntPtr)Marshal.SizeOf <NativePgfExprMeta>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<NativePgfExprMeta> (exprMetaPtr, (ref NativePgfExprMeta m) => m.Id = 0);
		}

		internal MetaVariable(IntPtr ptr, IntPtr pool) : base(ptr, pool) {	}


		public int Id => Data.Id;
	    private NativePgfExprMeta Data => Marshal.PtrToStructure<NativePgfExprMeta>(DataPtr);

		public override R Accept<R> (IVisitor<R> visitor)
		{
//			return visitor.VisitMetaVariable (Id);
			//Ignore metavariables
			throw new NotImplementedException();
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfExprMeta { public int Id; }
	}
}

