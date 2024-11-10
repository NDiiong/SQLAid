using System;
using System.Runtime.Serialization;

namespace SQLAid.Integration.Exceptions
{
    [Serializable]
    public class TypeConversionException : Exception
    {
        public TypeConversionException()
        {
        }

        public TypeConversionException(string message) : base(message)
        {
        }

        public TypeConversionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TypeConversionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}