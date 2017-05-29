using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PGF
{
    public class LiteralStringExpression : LiteralExpression
    {
        internal LiteralStringExpression(IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr, pool) { }
        public LiteralStringExpression(string s) : base()
        {
            _pool = new NativeGU.NativeMemoryPool();

            var exprTag = (byte)(int)PgfExprTag.PGF_EXPR_LIT;
            IntPtr litPtr = NativeGU.gu_alloc_variant(exprTag,
                (UIntPtr)Marshal.SizeOf<NativePgfExprLit>(), UIntPtr.Zero, ref _ptr, _pool.Ptr);

            Native.EditStruct<NativePgfExprLit>(litPtr, (ref NativePgfExprLit lit) => {
                MkStringVariant((byte)PgfLiteralTag.PGF_LITERAL_STR, s, ref lit.lit);
            });
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLiteralString(Value);
        }

        public string Value => Native.NativeString.StringFromNativeUtf8(LitDataPtr);
    }

    public class LiteralIntExpression : LiteralExpression
    {
        internal LiteralIntExpression(IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr, pool) { }
        public LiteralIntExpression(int val) : base()
        {
            Initialize<NativePgfLiteralInt>(PgfLiteralTag.PGF_LITERAL_INT,
                (ref NativePgfLiteralInt ilit) => ilit.val = val);
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLiteralInt(Value);
        }

        public int Value => Marshal.PtrToStructure<NativePgfLiteralInt>(LitDataPtr).val;

    }

    public class LiteralFloatExpression : LiteralExpression
    {
        internal LiteralFloatExpression(IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr, pool) { }
        public LiteralFloatExpression(double val) : base()
        {
            Initialize<NativePgfLiteralFlt>(PgfLiteralTag.PGF_LITERAL_FLT,
                (ref NativePgfLiteralFlt flit) => flit.val = val);
        }

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLiteralFloat(Value);
        }

        public double Value => Marshal.PtrToStructure<NativePgfLiteralFlt>(LitDataPtr).val;
    }

	public abstract class LiteralExpression : Expression
    {
        internal LiteralExpression(IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr, pool) { }
        protected LiteralExpression() { }

        internal new static Expression FromPtr(IntPtr expr, NativeGU.NativeMemoryPool pool)
        {
            var dataPtr = NativeGU.gu_variant_open(expr).Data; // PgfExprLit* 
            var data = Marshal.PtrToStructure<NativePgfExprLit>(dataPtr);
            var literalTag = (PgfLiteralTag)NativeGU.gu_variant_open(data.lit).Tag;

            switch(literalTag)
            {
                case PgfLiteralTag.PGF_LITERAL_STR:
                    return new LiteralStringExpression(expr, pool);
                case PgfLiteralTag.PGF_LITERAL_INT:
                    return new LiteralIntExpression(expr, pool);
                case PgfLiteralTag.PGF_LITERAL_FLT:
                    return new LiteralFloatExpression(expr, pool);
                default:
                    throw new ArgumentException();
            }
        }

        internal void Initialize<TNative>(PgfLiteralTag litTag, Native.StructAction<TNative> setValue, UIntPtr? size = null) {
            _pool = new NativeGU.NativeMemoryPool();

            var exprTag = (byte)(int)PgfExprTag.PGF_EXPR_LIT;
			IntPtr litPtr = NativeGU.gu_alloc_variant ( exprTag,
				(UIntPtr)Marshal.SizeOf<NativePgfExprLit>(), UIntPtr.Zero, ref _ptr, _pool.Ptr);

			Native.EditStruct<NativePgfExprLit> (litPtr, (ref NativePgfExprLit lit) => {
				IntPtr ilitPtr = NativeGU.gu_alloc_variant ((byte)litTag,
					(UIntPtr)Marshal.SizeOf<TNative> (), UIntPtr.Zero, ref lit.lit, _pool.Ptr);

				Native.EditStruct<TNative>(ilitPtr, setValue); 
			});
		}

		// Deref DatPtr to det PgfExprLit.
		private NativePgfExprLit Data => Marshal.PtrToStructure<NativePgfExprLit>(DataPtr);

		private PgfLiteralTag LiteralTag => (PgfLiteralTag) NativeGU.gu_variant_open(Data.lit).Tag;
		protected IntPtr LitDataPtr => NativeGU.gu_variant_open(Data.lit).Data;

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

