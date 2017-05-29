using System;
using System.Runtime.InteropServices;

namespace PGF
{
	public class MetaVariableExpression : Expression {

		public MetaVariableExpression() {
			_pool = new NativeGU.NativeMemoryPool();
			IntPtr exprMetaPtr = NativeGU.gu_alloc_variant ((byte)PgfExprTag.PGF_EXPR_META,
				(UIntPtr)Marshal.SizeOf <NativePgfExprMeta>(), UIntPtr.Zero, ref _ptr, _pool.Ptr);

			Native.EditStruct<NativePgfExprMeta> (exprMetaPtr, (ref NativePgfExprMeta m) => m.Id = 0);
		}

		internal MetaVariableExpression(IntPtr ptr, NativeGU.NativeMemoryPool pool) : base(ptr, pool) {	}


		public int Id => Data.Id;
	    private NativePgfExprMeta Data => Marshal.PtrToStructure<NativePgfExprMeta>(DataPtr);

		public override R Accept<R> (IVisitor<R> visitor)
		{
            //	return visitor.VisitMetaVariable (Id);

			// Not supported yet.
			throw new NotImplementedException();
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfExprMeta { public int Id; }
	}
}

