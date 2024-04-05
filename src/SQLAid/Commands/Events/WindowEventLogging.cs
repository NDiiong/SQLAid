using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Logging;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.Events
{
    public static class WindowEventLogging
    {
        private static WindowEvents _windowEvents;

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            //ThreadHelper.JoinableTaskFactory.RunAsync(async () => { });
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

            _windowEvents = dte.Events.get_WindowEvents();
            _windowEvents.WindowCreated += WindowEvents_WindowCreated;
            _windowEvents.WindowClosing += WindowEvents_WindowClosing;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
            _windowEvents.WindowMoved += WindowEvents_WindowMoved;
        }

        private static void WindowEvents_WindowMoved(Window Window, int Top, int Left, int Width, int Height)
        {
            Logger.Info("Call [WindowEvents_WindowMoved]");
        }

        private static void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            Logger.Info("Call [WindowEvents_WindowActivated]");
        }

        private static void WindowEvents_WindowClosing(Window Window)
        {
            Logger.Info("Call [WindowEvents_WindowClosing]");
        }

        private static void WindowEvents_WindowCreated(Window Window)
        {
            Logger.Info("Call [WindowEvents_WindowCreated]");
        }
    }
}