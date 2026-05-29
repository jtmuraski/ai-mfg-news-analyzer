using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfgNewsAnalyzer.Core.Exceptions
{
    public class InvalidApiKeyException : Exception
    {
        public InvalidApiKeyException() : base() { }
        public InvalidApiKeyException(string message) : base(message) { }

        public InvalidApiKeyException(string message, Exception innerException) : base(message, innerException) { }

        protected InvalidApiKeyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {}
    }
}
