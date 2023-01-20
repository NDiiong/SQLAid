using System;
using System.Text;

namespace SQLAid.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception exception)
        {
            var builder = new StringBuilder();
            Exception realerror = exception;
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