using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public class Grammar : IDisposable
    {
        private Grammar() { }

        IntPtr _pgf;
        IntPtr _GUPool;
        IntPtr _GUExn;
        public static Grammar FromFile(string fn)
        {
            var obj = new Grammar();
			using (var tmp_pool = new NativeGU.PoolErr ()) {
				obj._GUPool = NativeGU.gu_new_pool ();

				obj._pgf = Native.pgf_read (fn, obj._GUPool, tmp_pool.ErrPtr);
				if(tmp_pool.Exception) {
					throw new PGF.Exceptions.PGFException ($"Could not read PGF from file {fn}. ({System.IO.Directory.GetCurrentDirectory()})");
				}
				return obj;
			}
        }

        public string Name => Native.GetPermanentString(Native.pgf_abstract_name, _pgf);
        public string StartCat => Native.GetPermanentString(Native.pgf_start_cat, _pgf);

        public Dictionary<string, Concrete> Languages
        {
            get
            {
                var dict = new Dictionary<string, Concrete>();
				Native.MapIter(Native.pgf_iter_languages, _pgf, (k, v) => dict[k] = Concrete.FromPtr(this, dereference(v)));
                return dict;
            }
        }

		private IntPtr dereference(IntPtr ptr) {
			return (IntPtr) Marshal.PtrToStructure (ptr, typeof(IntPtr));
		}

		public IEnumerable<string> Categories => GetStringList(Native.pgf_iter_categories);
		public IEnumerable<string> Functions => GetStringList(Native.pgf_iter_functions);


		public IEnumerable<string> FunctionByCategory(string catName) => GetStringList(
			new Native.IterFuncCurryName(Native.pgf_iter_functions_by_cat, catName).IterFunc);

		public Type FunctionType(string funName) => Type.FromPtr(this, Native.pgf_function_type(_pgf, funName));

		public Expression ReadExpression(string exprStr) => 
			Read<Expression>(Native.pgf_read_expr, (a,b) => Expression.FromPtr(a,b), exprStr);

		public Type ReadType(string typeStr) => 
			Read<Type>(Native.pgf_read_type, (a,b) => Type.FromPtrs(a,b), typeStr);	

		public Expression Compute(Expression expr) {
			using (var pool = new NativeGU.PoolErr ()) {

				var newExprPool = NativeGU.gu_new_pool ();

				var newExpr = Native.pgf_compute (_pgf, expr.NativePtr, pool.ErrPtr, pool.Ptr, newExprPool);

				if (pool.Exception || newExpr == IntPtr.Zero) {
					NativeGU.gu_pool_free (newExprPool);
					throw new PGF.Exceptions.PGFException ("Could not reduce expression.");
				} else {
					return Expression.FromPtr (newExpr, newExprPool);
				}
			}
		}

		public IEnumerable<Expression> GenerateAll(string cat) {
			using(var pool = new NativeGU.PoolErr()) {
				var newExprPool = NativeGU.gu_new_pool ();
				var catNativeString = Marshal.StringToCoTaskMemAnsi (cat);

				IntPtr ptr = IntPtr.Zero;


				var iterator = Native.pgf_generate_all(_pgf, catNativeString, pool.ErrPtr, pool.Ptr, newExprPool);

				// Ptr is now of type PgfExprProb*.

				NativeGU.gu_enum_next(iterator, ref ptr, pool.Ptr);
				while (ptr != IntPtr.Zero) {

					// FIXME Get expression
					yield return null;

					NativeGU.gu_enum_next (iterator, ref ptr, pool.Ptr);
				}

				Marshal.FreeCoTaskMem (catNativeString);
			}
		}

		/*public Type ReadType(string typeStr) {
			
		}*/

		private T Read<T>(Func<IntPtr, IntPtr, IntPtr, IntPtr> reader, Func<IntPtr, IntPtr, T> factory, string str) {
			using (var pool = new NativeGU.PoolErr ()) {

				var strNative = Marshal.StringToCoTaskMemAnsi(str);

				var in_ = NativeGU.gu_data_in (strNative, str.Length, pool.Ptr);

				var exprPool = NativeGU.gu_new_pool();
				var expr = reader (in_, exprPool, pool.ErrPtr);

				if (pool.Exception || expr == IntPtr.Zero) {
					NativeGU.gu_pool_free (exprPool);
					Marshal.FreeCoTaskMem (strNative);
					throw new PGF.Exceptions.ParseErrorException ();
				} else {
					Marshal.FreeCoTaskMem (strNative);
					return factory(expr, exprPool);
				}
			}
		}

		private IEnumerable<string> GetStringList(Native.IterFunc f)
        {
            var c = new List<string>();
            Native.MapIter(f, _pgf, (k, v) => c.Add(k));
            return c;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    NativeGU.gu_pool_free(_GUPool);
                }

                _GUExn = IntPtr.Zero;
                _GUPool = IntPtr.Zero;


                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Grammar() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
