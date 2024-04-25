#pragma warning disable IDE1006

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.Logging;
using System.ComponentModel.Design;
using System.Drawing;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class WindowsOpenedAlertEnvironmentEvents
    {
        private static WindowEvents _windowEvents;
        private static readonly IFrameDocumentView _frameDocumentView;

        static WindowsOpenedAlertEnvironmentEvents()
        {
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            _windowEvents = package.Application.Events.get_WindowEvents();
            _windowEvents.WindowCreated += WindowEvents_WindowCreated;
            _windowEvents.WindowClosing += WindowEvents_WindowClosing;
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
        }

        private static void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            Logger.Info("Call [WindowEvents_WindowActivated]");

            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            var statusBarManager = sqlScriptEditorControl.StatusBarManager;
            var generalPanel = Reflection.GetField(statusBarManager, "generalPanel");
            var statusStrip = Reflection.GetField(statusBarManager, "statusStrip");

            var production_color = ColorTranslator.FromHtml("#ff8080");
            var dev_color = ColorTranslator.FromHtml("#80ff80");
            var normalColor = ColorTranslator.FromHtml("#fdf4bf");

            // Background Color
            Reflection.SetProperty(generalPanel, "BackColor", normalColor);
            Reflection.SetProperty(statusStrip, "BackColor", normalColor);

            //// Font Color
            //Reflection.SetProperty(statusStrip, "ForeColor", normalColor);
            //Reflection.SetProperty(statusStrip, "ForeColor", normalColor);
        }

        private static void WindowEvents_WindowClosing(Window Window)
        {
            Logger.Info("Call [WindowEvents_WindowClosing]");
        }

        private static void WindowEvents_WindowCreated(Window Window)
        {
            Logger.Info("Call [WindowEvents_WindowClosing]");
        }
    }
}