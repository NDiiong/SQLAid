using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SQLAid.Logging
{
    public static class Func
    {
        public static void Run(Action action, string message)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Run {message}");
            }
            finally
            {
                Logger.Info($"Run {message} - in {sw.ElapsedMilliseconds} ms");
            }
        }

        public static void Run(Action action, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

        public static T Run<T>(Func<T> action, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

        public static async Task<TResult> IgnoreAsync<TResult, TException>(this Task<TResult> task, TResult defaultValue) where TException : Exception
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                if (ex is TException)
                    return defaultValue;
                throw;
            }
        }

        public static async Task<TResult> IgnoreAsync<TResult, TException>(this Task task, TResult defaultValue) where TException : Exception
        {
            try
            {
                return await (Task<TResult>)task;
            }
            catch (Exception ex)
            {
                if (ex is TException)
                    return defaultValue;
                throw;
            }
        }

        public static void Ignore<TException>(this Action action) where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex) when (ex is TException)
            {
            }
        }

        public static Task IgnoreAsync<TException>(this Task task) where TException : Exception
        {
            try
            {
                Func<Task> awaitableCallback = async () => await task;
                awaitableCallback();
                return task;
            }
            catch (Exception ex)
            {
                if (ex is TException)
                    return task;
                throw;
            }
        }
    }
}