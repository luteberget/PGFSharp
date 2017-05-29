using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    /// <summary>
    /// Grammatical Framework grammar.
    /// </summary>
    public class Grammar
    {
        private Grammar() { }

        IntPtr _ptr;
        NativeGU.NativeMemoryPool pool;

        /// <summary>
        /// Read grammar from PGF file.
        /// </summary>
        /// <param name="fn">filename</param>
        /// <returns></returns>
        public static Grammar FromFile(string fn)
        {
            var obj = new Grammar();
            var exn = new NativeGU.NativeExceptionContext(new NativeGU.NativeMemoryPool());
            obj.pool = new NativeGU.NativeMemoryPool();
            obj._ptr = Native.pgf_read(fn, obj.pool.Ptr, exn.Ptr);
            if (exn.IsRaised)
            {
                throw new PGF.Exceptions.PGFException($"Could not read PGF from file {fn}. ({System.IO.Directory.GetCurrentDirectory()})");
            }
            return obj;
        }


        public override string ToString() => $"Grammar:{Name}, {String.Join(", ", Languages.Keys)}";

        /// <summary>
        /// Name of the abstract grammar.
        /// </summary>
        public string Name => Native.NativeString.StringFromNativeUtf8(Native.pgf_abstract_name(_ptr));

        /// <summary>
        /// Default category of the grammar.
        /// </summary>
        public Type StartCat => Type.FromPtr(Native.pgf_start_cat(_ptr, pool.Ptr), pool);

        /// <summary>
        /// All concrete grammars in the language.
        /// </summary>
        public Dictionary<string, Concrete> Languages
        {
            get
            {
                var dict = new Dictionary<string, Concrete>();
                Native.MapIter(Native.pgf_iter_languages, _ptr, (k, v) => dict[k] = Concrete.FromPtr(this, dereference(v)));
                return dict;
            }
        }

        private IntPtr dereference(IntPtr ptr)
        {
            return (IntPtr)Marshal.PtrToStructure(ptr, typeof(IntPtr));
        }

        /// <summary>
        /// All categories in the abstract grammar.
        /// </summary>
        public IEnumerable<string> Categories => GetStringList(Native.pgf_iter_categories);

        /// <summary>
        /// All functions in the abstract grammar.
        /// </summary>
        public IEnumerable<string> Functions => GetStringList(Native.pgf_iter_functions);

        /// <summary>
        /// All functions producing the given category name.
        /// </summary>
        /// <param name="catName"></param>
        /// <returns></returns>
        public IEnumerable<string> FunctionByCategory(string catName)
        {
            using (var str = new Native.NativeString(catName))
            {
                return GetStringList(new Native.IterFuncCurryName(Native.pgf_iter_functions_by_cat, str.Ptr).IterFunc);
            }
        }

        /// <summary>
        /// Get type from function name.
        /// </summary>
        /// <param name="funName"></param>
        /// <returns></returns>
        public Type FunctionType(string funName)
        {
            using (var str = new Native.NativeString(funName))
            {
                var typePtr = Native.pgf_function_type(_ptr, str.Ptr);
                if (typePtr == IntPtr.Zero) throw new NullReferenceException();
                return Type.FromPtr(typePtr, pool);
            }
        }

        /// <summary>
        /// Read expression from string.
        /// </summary>
        /// <param name="exprStr"></param>
        /// <returns></returns>
        public Expression ReadExpression(string exprStr) =>
            Read(Native.pgf_read_expr, (a, b) => Expression.FromPtr(a, b), exprStr);

        /// <summary>
        /// Read type from string.
        /// </summary>
        /// <param name="typeStr"></param>
        /// <returns></returns>
        public Type ReadType(string typeStr) =>
            Read(Native.pgf_read_type, (a, b) => Type.FromPtr(a, b), typeStr);

        /// <summary>
        /// Reduce expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Expression Compute(Expression expr)
        {
            var tmp_pool = new NativeGU.NativeMemoryPool();
            var exn = new NativeGU.NativeExceptionContext(tmp_pool);
            var result_pool = new NativeGU.NativeMemoryPool();
            var newExpr = Native.pgf_compute(_ptr, expr.Ptr, exn.Ptr, pool.Ptr, result_pool.Ptr);

            if (exn.IsRaised || newExpr == IntPtr.Zero)
            {
                throw new PGF.Exceptions.PGFException("Could not reduce expression.");
            }
            else
            {
                return Expression.FromPtr(newExpr, result_pool);
            }
        }

        /// <summary>
        /// Enumerate all expressions in the given category.
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        public IEnumerable<Expression> GenerateAll(Type cat = null)
        {
            cat = cat ?? StartCat;
            var tmp_pool = new NativeGU.NativeMemoryPool();
            var exn = new NativeGU.NativeExceptionContext(tmp_pool);
            var result_pool = new NativeGU.NativeMemoryPool();
            IntPtr ptr = IntPtr.Zero;
            var iterator = Native.pgf_generate_all(this._ptr, cat.Ptr, exn.Ptr, tmp_pool.Ptr, result_pool.Ptr);

            return NativeGU.IteratorToIEnumerable(iterator, tmp_pool.Ptr).Select(p =>
            {
                var exprProb = Marshal.PtrToStructure<Native.PgfExprProb>(ptr);
                return Expression.FromPtr(exprProb.expr, result_pool);

            });
        }

        private T Read<T>(Func<IntPtr, IntPtr, IntPtr, IntPtr> reader, Func<IntPtr, NativeGU.NativeMemoryPool, T> factory, string str)
        {
            var tmp_pool = new NativeGU.NativeMemoryPool();
            var exn = new NativeGU.NativeExceptionContext(tmp_pool);
            var result_pool = new NativeGU.NativeMemoryPool();
            using (var strNative = new Native.NativeString(str))
            {
                var in_ = NativeGU.gu_data_in(strNative.Ptr, strNative.Size, tmp_pool.Ptr);
                var expr = reader(in_, result_pool.Ptr, exn.Ptr);
                if (exn.IsRaised || expr == IntPtr.Zero)
                {
                    throw new PGF.Exceptions.ParseErrorException();
                }
                else
                {
                    return factory(expr, result_pool);
                }
            }
        }

        private IEnumerable<string> GetStringList(Native.MapIterFunc f)
        {
            var c = new List<string>();
            Native.MapIter(f, _ptr, (k, v) => c.Add(k));
            return c;
        }
    }
}
