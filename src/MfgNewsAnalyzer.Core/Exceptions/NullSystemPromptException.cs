using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Exceptions
{
    public class NullSystemPromptException : Exception
    {
        public NullSystemPromptException() : base() { }
        public NullSystemPromptException(string message) : base(message) { }

        public NullSystemPromptException(string message, Exception innerException) : base(message, innerException) { }

        protected NullSystemPromptException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {}
    }
}
