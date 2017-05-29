using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public class UnsupportedExpression : Expression
    {
        internal UnsupportedExpression(IntPtr expr, NativeGU.NativeMemoryPool pool) : base(expr, pool) { }
        public override R Accept<R>(IVisitor<R> visitor)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Expression
    {
        protected IntPtr DataPtr => NativeGU.gu_variant_open(_ptr).Data; // PgfExprLit* 
        protected PgfExprTag Tag => (PgfExprTag)NativeGU.gu_variant_open(_ptr).Tag;

        protected IntPtr MkStringVariant(byte tag, string s, ref IntPtr out_)
        {
            var size = Encoding.UTF8.GetByteCount(s);
            IntPtr slitPtr = NativeGU.gu_alloc_variant(tag,
                (UIntPtr)(size + 1), UIntPtr.Zero, ref out_, _pool.Ptr);
            Native.NativeString.CopyToPreallocated(s, slitPtr);
            return slitPtr;
        }

        public enum PgfExprTag
        {
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

        public interface IVisitor<R>
        {
            R VisitLiteralInt(int value);
            R VisitLiteralFloat(double value);
            R VisitLiteralString(string value);
            R VisitApplication(string fname, Expression[] args);

            //R VisitMetaVariable (int id); Dont' care about this for now...

            // Remove this, Function objects use VisitApplication with empty args instead.
            //R VisitFunction (string fname); // Will this be used?
        }

        public class Visitor<R> : IVisitor<R>
        {

            public Func<int, R> fVisitLiteralInt { get; set; } = null;
            public R VisitLiteralInt(int x1) => fVisitLiteralInt(x1);
            public Func<double, R> fVisitLiteralFlt { get; set; } = null;
            public R VisitLiteralFloat(double x1) => fVisitLiteralFlt(x1);
            public Func<string, R> fVisitLiteralStr { get; set; } = null;
            public R VisitLiteralString(string x1) => fVisitLiteralStr(x1);
            public Func<string, Expression[], R> fVisitApplication { get; set; } = null;
            public R VisitApplication(string x1, Expression[] x2) => fVisitApplication(x1, x2);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

        protected IntPtr _ptr = IntPtr.Zero;
        internal NativeGU.NativeMemoryPool _pool;
        internal IntPtr Ptr => _ptr;

        protected Expression() { }
         internal Expression(IntPtr ptr, NativeGU.NativeMemoryPool pool)
        {
            _ptr = ptr; _pool = pool;
        }

        // Factories
        private static Dictionary<PgfExprTag, Func<IntPtr, NativeGU.NativeMemoryPool, Expression>> factories =
            new Dictionary<PgfExprTag, Func<IntPtr, NativeGU.NativeMemoryPool, Expression>>{

            { PgfExprTag.PGF_EXPR_LIT, (e, p) => LiteralExpression.FromPtr (e, p) },
            { PgfExprTag.PGF_EXPR_APP, (e, p) => new ApplicationExpression (e, p) },
            { PgfExprTag.PGF_EXPR_FUN, (e, p) => new FunctionExpression (e, p) },
            { PgfExprTag.PGF_EXPR_META, (e, p) => new MetaVariableExpression (e, p) }
        };

        internal static Expression FromPtr(IntPtr expr, NativeGU.NativeMemoryPool pool)
        {
            var Tag = (PgfExprTag)NativeGU.gu_variant_open(expr).Tag;
            if (factories.ContainsKey(Tag))
            {
                return factories[Tag](expr, pool);
            }
            else
                return new UnsupportedExpression(expr, pool);
        }

        public override string ToString() =>
            Native.ReadString((output,exn) => Native.pgf_print_expr(_ptr, IntPtr.Zero, 0, output, exn.Ptr));

    }
}
