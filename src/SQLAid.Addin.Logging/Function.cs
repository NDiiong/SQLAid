using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SQLAid.Addin.Logging
{
    public static class Function
    {
        public static void Run(Action action,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                var message = $"Invoke method {memberName} | {Path.GetFileName(sourceFilePath)} | Line: {sourceLineNumber} - in {sw.ElapsedMilliseconds} ms";
                Logger.Info(message);
            }
        }

        public static T Run<T>(Func<T> action,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                var message = $"Invoke method {memberName} | {Path.GetFileName(sourceFilePath)} | Line: {sourceLineNumber} - in {sw.ElapsedMilliseconds} ms";
                Logger.Info(message);
            }

            return default;
        }
    }
}