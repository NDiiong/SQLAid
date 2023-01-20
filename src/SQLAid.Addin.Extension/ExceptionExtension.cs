using System;
using System.Text;

namespace SQLAid.Addin.Extension
{
    public static class ExceptionExtension
    {
        public static string GetFullMessage(this Exception exception)
        {
            var builder = new StringBuilder();
            var realerror = exception;
            builder.AppendLine(exception.Message);
            while (realerror.InnerException != null)
            {
                builder.AppendLine(realerror.InnerException.Message);
                realerror = realerror.InnerException;
            }
            return builder.ToString();
        }
    }
}