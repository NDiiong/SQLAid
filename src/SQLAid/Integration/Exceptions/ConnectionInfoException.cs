using System;

namespace SQLAid.Integration.Exceptions
{
    public class ConnectionInfoException : Exception
    {
        public ConnectionInfoException(string message) : base(message)
        {
        }

        public ConnectionInfoException() : base()
        {
        }

        public ConnectionInfoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}