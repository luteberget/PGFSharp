using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Portable grammar format PInvoke functions.
/// </summary>
namespace PGF
{
    static public class Native {

		internal class NativeString : IDisposable {
			internal IntPtr Ptr;
			public NativeString(string s) {
				Ptr = NativeUtf8FromString(s);
			}

			public void Dispose() {
				Marshal.FreeHGlobal (Ptr);
				Ptr = IntPtr.Zero;
			}

			public static IntPtr NativeUtf8FromString(string managedString) {
				int len = Encoding.UTF8.GetByteCount(managedString);
				byte[] buffer = new byte[len + 1];
				Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
				IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
				Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
				return nativeUtf8;
			}

			public static string StringFromNativeUtf8(IntPtr nativeUtf8) {
				int len = 0;
				while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
				byte[] buffer = new byte[len];
				Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
				return Encoding.UTF8.GetString(buffer);
			}
		}

		public static void EditStruct<T>(IntPtr ptr, Func<T, T> f) {
			var str = Marshal.PtrToStructure<T> (ptr);
			str = f (str);
			Marshal.StructureToPtr<T> (str, ptr, false);
		}

        const string LIBNAME = "pgf.dll";
        const CallingConvention CC = CallingConvention.Cdecl;


		#region Basic
        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr pgf_read([MarshalAs(UnmanagedType.LPStr)] string fpath, IntPtr pool, IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr pgf_read_in(IntPtr in_, IntPtr pool, IntPtr tmp_pool, IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void pgf_concrete_load(IntPtr concr, IntPtr in_, IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr pgf_abstract_name(IntPtr pgf);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr pgf_start_cat(IntPtr pgf);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void pgf_print_expr(IntPtr expr, IntPtr ctxt, int prec, IntPtr output, IntPtr err);
		#endregion


		#region Iteration
        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void pgf_iter_languages(IntPtr pgf, ref GuMapItor itor, IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void pgf_iter_categories(IntPtr pgf, ref GuMapItor itor, IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void pgf_iter_functions(IntPtr pgf, ref GuMapItor itor, IntPtr err);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern void pgf_iter_functions_by_cat (IntPtr pgf, [MarshalAs (UnmanagedType.LPStr)] string catName, ref GuMapItor itor, IntPtr err);

		#endregion

		#region Type
		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_function_type (IntPtr pgf, [MarshalAs (UnmanagedType.LPStr)] string funName);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_read_type(IntPtr in_, IntPtr pool, IntPtr err);
		#endregion

		#region Expression
		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_read_expr(IntPtr in_, IntPtr pool, IntPtr err);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_compute (IntPtr pgf, IntPtr expr, IntPtr err, IntPtr tmp_pool, IntPtr res_pool);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_generate_all (IntPtr pgf, IntPtr catName, IntPtr err, IntPtr iter_pool, IntPtr out_pool);
		#endregion

		#region Concrete
		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_parse_with_heuristics (IntPtr concr, IntPtr cat, IntPtr sentence, 
		                                                       double heuristics, IntPtr callbacks, IntPtr exn, 
		                                                       IntPtr parsePl, IntPtr exprPl);
		#endregion

		#region Callbacks
		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr pgf_new_callbacks_map (IntPtr concr, IntPtr pool);
		#endregion

        public static string GetPermanentString(Func<IntPtr,IntPtr> f, IntPtr p)
        {
            var ptr = f(p);
            return Marshal.PtrToStringAnsi(ptr);
        }

        //public delegate void GuMapItorFn(IntPtr self, [MarshalAs(UnmanagedType.LPStr)] string key, IntPtr value, IntPtr err);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GuMapItorFn(IntPtr self, IntPtr key, IntPtr value, IntPtr err);


        /*
        public delegate void GuFinalizerFn(IntPtr self);

        [StructLayout(LayoutKind.Sequential)]
        public struct GuFinalizer
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public GuFinalizerFn fn;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PgfConcr
        {
            public IntPtr name, abstr, cflags, printnames,
                ccats, fun_indices, coerce_idx, cncfuns,
                sequences, cnccats;
            public int total_cats;
            public IntPtr pool;
            GuFinalizer fin;
        }*/

        [StructLayout(LayoutKind.Sequential)]
        public struct GuMapItor
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public GuMapItorFn fn;
        }

		[StructLayout(LayoutKind.Sequential)]
		public struct PgfExprProb {
			public float prob;
			public IntPtr expr; // PgfExpr type (not pointer, but typedef PgfExpr -> GuVariant -> uintptr_t)
		}

        /*
        [StructLayout(LayoutKind.Sequential)]
        public struct PGFClosure
        {
            public GuMapItor fn;
            public IntPtr grammar;
            public IntPtr obj;
        }*/

        public delegate void IterFunc(IntPtr pgf, ref GuMapItor fn, IntPtr err);
		public delegate void IterNameFunc(IntPtr pgf, string name, ref GuMapItor fn, IntPtr err);

		public class IterFuncCurryName
		{
			private string name;
			private IterNameFunc func;
			public IterFuncCurryName(IterNameFunc f, string name) {
				this.func = f;
				this.name = name;
			}

			public void IterFunc(IntPtr pgf, ref GuMapItor fn, IntPtr err) {
				func (pgf, name, ref fn, err);
			}
		}

        public static void MapIter(IterFunc iter, IntPtr _pgf, Action<string,IntPtr> action)
        {
            var pool = NativeGU.gu_new_pool();
            var err = NativeGU.gu_new_exn(pool);
            var f = new GuMapItor()
            {
                fn = (self, key, value, _err) =>
                {
                    action(Marshal.PtrToStringAnsi(key), value);
                }
            };

            iter(_pgf, ref f, err);
            NativeGU.gu_pool_free(pool);
        }
    }
}
