using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Exceptions
{
    public class PromptFailureException : Exception
    {
        public PromptFailureException() : base() { }
        public PromptFailureException(string message) : base(message) { }

        public PromptFailureException(string message, Exception innerException) : base(message, innerException) { }
        
        public PromptFailureException (System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

    }
}
