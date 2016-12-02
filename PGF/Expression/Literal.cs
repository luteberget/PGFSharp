using System;
using System.Runtime.InteropServices;

namespace PGF
{
	public class Meta : Expression {
		// initMeta
		public Meta() : base() {
			_pool = NativeGU.gu_new_pool ();
			IntPtr exprMetaPtr = NativeGU.gu_alloc_variant ((byte)PgfExprTag.PGF_EXPR_META,
				(UIntPtr)Marshal.SizeOf <NativePgfExprMeta>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<NativePgfExprMeta> (exprMetaPtr, m => { m.id = 0; return m; } );
		}

		public override R Accept<R> (IVisitor<R> visitor)
		{
			return visitor.VisitMetaVariable (0);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfExprMeta { public int id; }
	}
	
	public class Literal : Expression
	{
		// initLiteral
		public Literal (int val)
		{
			_pool = NativeGU.gu_new_pool ();

			var exprTag = (byte)(int)PgfExprTag.PGF_EXPR_LIT;
			IntPtr litPtr = NativeGU.gu_alloc_variant ( exprTag,
				(UIntPtr)Marshal.SizeOf<NativePgfExprLit>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<NativePgfExprLit> (litPtr, lit => {
				var litTag = (byte)(int)NativePgfLiteralTag.PGF_LITERAL_INT;
				IntPtr ilitPtr = NativeGU.gu_alloc_variant (litTag,
					(UIntPtr)Marshal.SizeOf<NativePgfLiteralInt> (), UIntPtr.Zero, ref lit.lit, _pool);

				Native.EditStruct<NativePgfLiteralInt>(ilitPtr, ilit => { ilit.val = val; return ilit; }); 

				return lit;
			});
		}

		public override R Accept<R> (IVisitor<R> visitor)
		{
			return visitor.VisitLiteral (Value);
		}

			public object Value {
			get {
				switch (LiteralTag) {
				case NativePgfLiteralTag.PGF_LITERAL_STR:
					var _str = Marshal.PtrToStructure<NativePgfLiteralStr> (LitDataPtr);
					return Native.NativeString.StringFromNativeUtf8(_str.val);
				case NativePgfLiteralTag.PGF_LITERAL_INT:
					var _int = Marshal.PtrToStructure<NativePgfLiteralInt>(LitDataPtr);
					return _int.val;
				case NativePgfLiteralTag.PGF_LITERAL_FLT:
					var _flt = Marshal.PtrToStructure<NativePgfLiteralFlt>(LitDataPtr);
					return _flt.val;
				default:
					throw new ArgumentException();
				}
			}
		}

		public static object GetValue(IntPtr ptr) {
			var variant = NativeGU.gu_variant_open(ptr);
			var Tag = (NativePgfLiteralTag) variant.Tag;
			var Data =  variant.Data;
			var i = (int)Tag;
			switch (Tag) {
			case NativePgfLiteralTag.PGF_LITERAL_STR:
				var _str = Marshal.PtrToStructure<NativePgfLiteralStr> (Data);
				return Native.NativeString.StringFromNativeUtf8(_str.val);
			case NativePgfLiteralTag.PGF_LITERAL_INT:
				var _int = Marshal.PtrToStructure<NativePgfLiteralInt>(Data);
				return _int.val;
			case NativePgfLiteralTag.PGF_LITERAL_FLT:
				var _flt = Marshal.PtrToStructure<NativePgfLiteralFlt>(Data);
				return _flt.val;
			default:
				throw new ArgumentException();
			}
		}
		// Expr
		private IntPtr DataPtr => NativeGU.gu_variant_open(_expr).Data; // PgfExprLit* 
		private PgfExprTag Tag => (PgfExprTag) NativeGU.gu_variant_open(_expr).Tag;

		// Deref DatPtr to det PgfExprLit.
		private NativePgfExprLit Data => Marshal.PtrToStructure<NativePgfExprLit>(DataPtr);

		private NativePgfLiteralTag LiteralTag => (NativePgfLiteralTag) NativeGU.gu_variant_open(Data.lit).Tag;
		private IntPtr LitDataPtr => NativeGU.gu_variant_open(Data.lit).Data;

		public enum NativePgfLiteralTag {
			PGF_LITERAL_STR,
			PGF_LITERAL_INT,
			PGF_LITERAL_FLT,
			PGF_LITERAL_NUM_TAGS
		}
		


		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfExprLit { public IntPtr lit; }

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfLiteralStr {	public IntPtr val; }

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfLiteralInt {	public int val; }

		[StructLayout(LayoutKind.Sequential)]
		public struct NativePgfLiteralFlt {	public double val; }
	}
}

