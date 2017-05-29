using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGF.Exceptions
{

    [Serializable]
    public class PGFException : Exception
    {
        public PGFException() { }
        public PGFException(string message) : base(message) { }
        public PGFException(string message, Exception inner) : base(message, inner) { }
        protected PGFException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }


    [Serializable]
    public class ParseErrorException : Exception
    {
        public ParseErrorException() { }
        public ParseErrorException(string message) : base(message) { }
        public ParseErrorException(string message, Exception inner) : base(message, inner) { }
        protected ParseErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

    [Serializable]
    public class TypeErrorException : Exception
    {
        public TypeErrorException() { }
        public TypeErrorException(string message) : base(message) { }
        public TypeErrorException(string message, Exception inner) : base(message, inner) { }
        protected TypeErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
