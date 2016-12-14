using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public static class NativeGU
    {

        const string LIBNAME = "gu.dll";
        const CallingConvention CC = CallingConvention.Cdecl;

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_new_pool();

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_new_exn(IntPtr pool);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern void gu_pool_free(IntPtr pool);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr get_gu_null_variant();

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_string_buf_out(IntPtr sbuf);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_string_buf(IntPtr pool);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr gu_data_in (IntPtr str, int len, IntPtr pool);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_string_buf_freeze(IntPtr sbuf, IntPtr pool);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern bool gu_exn_is_raised(IntPtr err);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern UIntPtr gu_seq_length(IntPtr seq);

        [DllImport(LIBNAME, CallingConvention = CC)]
        public static extern IntPtr gu_seq_data(IntPtr seq);

        [DllImport(LIBNAME, CallingConvention = CC)]
		public static extern void gu_enum_next (IntPtr enum_, ref IntPtr outPtr, IntPtr pool);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern GuVariantInfo gu_variant_open (IntPtr variant);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr gu_alloc_variant (byte tag, UIntPtr size, UIntPtr align, ref IntPtr out_, IntPtr pool);

		[DllImport(LIBNAME, CallingConvention = CC)]
		public static extern IntPtr gu_make_variant (byte tag, UIntPtr size, UIntPtr align, ref IntPtr init, IntPtr pool);

		[StructLayout(LayoutKind.Sequential)]
		public struct GuVariantInfo{
			public int Tag;
			public IntPtr Data;
		}

        public static T gu_seq_index<T>(IntPtr seq, int index)
        {
            var seqPtr = NativeGU.gu_seq_data(seq);
            var hypoPtr = seqPtr + index * Marshal.SizeOf<T>();
            var hypo = Marshal.PtrToStructure<T>(hypoPtr);
            return hypo;
        }

		public class PoolErr : IDisposable {

			public IntPtr Ptr;
			public IntPtr ErrPtr;
			public PoolErr() {
				Ptr = gu_new_pool();
				ErrPtr = gu_new_exn(Ptr);
			}

			public bool Exception => gu_exn_is_raised(ErrPtr);

			public void Dispose() {
				gu_pool_free (Ptr);
				Ptr = IntPtr.Zero;
				ErrPtr = IntPtr.Zero;
			}
		}

		public static IEnumerable<IntPtr> Iterate(IntPtr iterator, IntPtr pool) {
			IntPtr ptr = IntPtr.Zero;
			NativeGU.gu_enum_next (iterator, ref ptr, pool);
			while (ptr != IntPtr.Zero) {
				yield return ptr;
				NativeGU.gu_enum_next (iterator, ref ptr, pool);
			}
		}

    }
}
