using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using SQLAid.Integration.DTE;
using System;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor.Highlighter
{
    public static class HighlightCommand
    {
        private static WindowEvents _windowEvents;
        private static IServiceProvider _serviceProvider;

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _serviceProvider = package;
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            _windowEvents = dte.Events.WindowEvents;
            _windowEvents.WindowCreated += _windowEvents_WindowCreated;
        }

        private static void _windowEvents_WindowCreated(Window Window)
        {
            try
            {
                var vsTextManager = _serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
                var componentModel = (IComponentModel)_serviceProvider.GetService(typeof(SComponentModel));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                throw;
            }

            //throw new NotImplementedException();
        }
    }
}