using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    /// <summary>
    /// A GF type.
    /// </summary>
    public class Type
    {
        private IntPtr _ptr;
        internal IntPtr Ptr => _ptr;
        private NativeGU.NativeMemoryPool _pool;
        private Type() { }

        internal static Type FromPtr(IntPtr type, NativeGU.NativeMemoryPool pool)
        {
            var t = new Type();
            t._ptr = type;
            t._pool = pool;
            return t;
        }

        public override string ToString() =>
            Native.ReadString((output,exn) => Native.pgf_print_type(_ptr, IntPtr.Zero, 0, output, exn.Ptr));

        private PgfType Data => Marshal.PtrToStructure<PgfType>(_ptr);

        /// <summary>
        /// Get the hypotheses of a type (function argument types).
        /// </summary>
        public IEnumerable<Type> Hypotheses
        {
            get
            {
                var n_hypos = NativeGU.SeqLength(Data.hypos);
                for (int i = 0; i < n_hypos; i++)
                {
                    var hypo = NativeGU.gu_seq_index<PgfHypo>(Data.hypos, i);
                    var type = Type.FromPtr(hypo.type, this._pool);
                    yield return type;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct PgfType
        {
            public IntPtr hypos; // GuSeq of PgfHypo
            public IntPtr cid;
            public UIntPtr n_exprs;
            public IntPtr exprs;
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct PgfHypo
        {
            public int pgfBindType; // enum
            public IntPtr cid; // PgfCId (string)
            public IntPtr type; // PgfType*
        }
    }
}
