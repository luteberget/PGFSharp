using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PGF
{
	public class Literal : Expression
	{
		// initLiteral
		public Literal (int val)
		{
			Initialize<NativePgfLiteralInt> (PgfLiteralTag.PGF_LITERAL_INT,
				(ref NativePgfLiteralInt ilit) => ilit.val = val);
		}

		public Literal(double val) {
			Initialize<NativePgfLiteralFlt> (PgfLiteralTag.PGF_LITERAL_FLT,
				(ref NativePgfLiteralFlt flit) => flit.val = val);
		}

		public Literal(string s) {
			_pool = NativeGU.gu_new_pool ();

			var exprTag = (byte)(int)PgfExprTag.PGF_EXPR_LIT;
			IntPtr litPtr = NativeGU.gu_alloc_variant ( exprTag,
				(UIntPtr)Marshal.SizeOf<NativePgfExprLit>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<NativePgfExprLit> (litPtr, (ref NativePgfExprLit lit) => {
				MkStringVariant((byte)PgfLiteralTag.PGF_LITERAL_STR, s, ref lit.lit);
			});
		}

		internal Literal(IntPtr ptr, IntPtr pool) : base(ptr, pool) {	}


		protected void Initialize<TNative>(PgfLiteralTag litTag, Native.StructAction<TNative> setValue, UIntPtr? size = null) {
			_pool = NativeGU.gu_new_pool ();

			var exprTag = (byte)(int)PgfExprTag.PGF_EXPR_LIT;
			IntPtr litPtr = NativeGU.gu_alloc_variant ( exprTag,
				(UIntPtr)Marshal.SizeOf<NativePgfExprLit>(), UIntPtr.Zero, ref _expr, _pool);

			Native.EditStruct<NativePgfExprLit> (litPtr, (ref NativePgfExprLit lit) => {
				IntPtr ilitPtr = NativeGU.gu_alloc_variant ((byte)litTag,
					(UIntPtr)Marshal.SizeOf<TNative> (), UIntPtr.Zero, ref lit.lit, _pool);

				Native.EditStruct<TNative>(ilitPtr, setValue); 
			});
		}

		public override R Accept<R> (IVisitor<R> visitor)
		{
			switch (LiteralTag) {
			case PgfLiteralTag.PGF_LITERAL_STR:
				return visitor.VisitLiteralStr (Value as string);
			case PgfLiteralTag.PGF_LITERAL_INT:
				return visitor.VisitLiteralInt ((int)Value);
			case PgfLiteralTag.PGF_LITERAL_FLT:
				return visitor.VisitLiteralFlt ( (double)Value);
			default:
				throw new ArgumentException();
			}
		}

			public object Value {
			get {
				switch (LiteralTag) {
				case PgfLiteralTag.PGF_LITERAL_STR:
					//var _str = Marshal.PtrToStructure<NativePgfLiteralStr> (LitDataPtr);
					return Native.NativeString.StringFromNativeUtf8(LitDataPtr);
				case PgfLiteralTag.PGF_LITERAL_INT:
					var _int = Marshal.PtrToStructure<NativePgfLiteralInt>(LitDataPtr);
					return _int.val;
				case PgfLiteralTag.PGF_LITERAL_FLT:
					var _flt = Marshal.PtrToStructure<NativePgfLiteralFlt>(LitDataPtr);
					return _flt.val;
				default:
					throw new ArgumentException();
				}
			}
		}


		// Expr

		// Deref DatPtr to det PgfExprLit.
		private NativePgfExprLit Data => Marshal.PtrToStructure<NativePgfExprLit>(DataPtr);

		private PgfLiteralTag LiteralTag => (PgfLiteralTag) NativeGU.gu_variant_open(Data.lit).Tag;
		private IntPtr LitDataPtr => NativeGU.gu_variant_open(Data.lit).Data;

		public enum PgfLiteralTag {
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

