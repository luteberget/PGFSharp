using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PGF
{
    public class Type
    {
        private Grammar Grammar;
        private IntPtr _type;

        private IntPtr _pool = IntPtr.Zero; // FIXME: Initialized to null? See Python wrapper: PGF_functionType

        private Type(Grammar grammar, IntPtr ptr)
        {
            this.Grammar = grammar;
            this._type = ptr;
        }

        private Type(IntPtr type, IntPtr pool)
        {
        }

        internal static Type FromPtr(Grammar grammar, IntPtr type)
        {
            return new Type(grammar, type);
        }

        internal static Type FromPtrs(IntPtr type, IntPtr pool)
        {
            return new Type(type, pool);
        }

        public override string ToString() =>
            Native.ReadString((a, b, c, d) => Native.pgf_print_type(_type, a, b, c, d));

        private PgfType Data => Marshal.PtrToStructure<PgfType>(_type);

        public IEnumerable<Type> Hypotheses
        {
            get
            {
                var n_hypos = (uint)NativeGU.gu_seq_length(Data.hypos);
                for (int i = 0; i < n_hypos; i++)
                {
                    var hypo = NativeGU.gu_seq_index<PgfHypo>(Data.hypos, i);
                    var type = new Type(this.Grammar, hypo.type);
                    yield return type;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PgfType
        {
            public IntPtr hypos; // GuSeq of PgfHypo
            public IntPtr cid;
            public UIntPtr n_exprs;
            public IntPtr exprs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PgfHypo
        {
            public int pgfBindType; // enum
            public IntPtr cid; // PgfCId (string)
            public IntPtr type; // PgfType*
        }
    }
}
