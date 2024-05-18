#pragma warning disable IDE1006

using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Connection;
using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeihanLi.Extensions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class WindowsOpenedAlertEnvironmentEvents
    {
        private const string YELLOW_COLOR = "#f0e68c";
        private const string RED_COLOR = "#ff8080";
        private const string GREEN_COLOR = "#80ff80";

        private static WindowEvents _windowEvents;
        private static StatusStrip _statusStripCached;
        private static SqlAsyncPackage _sqlAsyncPackage;
        private static readonly ISqlConnection _sqlConnection;
        private static readonly IFrameDocumentView _frameDocumentView;

        static WindowsOpenedAlertEnvironmentEvents()
        {
            _frameDocumentView = new FrameDocumentView();
            _sqlConnection = new SqlConnection();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _sqlAsyncPackage = package;

            var commandService = _sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            _windowEvents = _sqlAsyncPackage.Application.Events.get_WindowEvents();
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
            _windowEvents.WindowCreated += WindowEvents_WindowCreated;
        }

        private static void WindowEvents_WindowCreated(Window Window)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // this is SqlScriptEditorControl class
            var sqlResultsControl = Window.Object.GetField("m_sqlResultsControl");

            if (sqlResultsControl != null)
            {
                var targetType = sqlResultsControl.GetType();
                if (targetType != null)
                {
                    var scriptExecutionCompletedEvent = targetType.GetEvent("ScriptExecutionCompleted");
                    if (scriptExecutionCompletedEvent != null)
                    {
                        EventHandler eventHandler = UpdateStatusStripBackColorEventHandler;
                        var handlerDelegate = Delegate.CreateDelegate(scriptExecutionCompletedEvent.EventHandlerType, eventHandler.Target, eventHandler.Method);
                        scriptExecutionCompletedEvent.RemoveEventHandler(sqlResultsControl, handlerDelegate);
                        scriptExecutionCompletedEvent.AddEventHandler(sqlResultsControl, handlerDelegate);
                    }

                    var scriptExecutionStarted = targetType.GetEvent("ScriptExecutionStarted");
                    if (scriptExecutionStarted != null)
                    {
                        EventHandler eventHandler = UpdateStatusStripBackColorEventHandler;
                        var handlerDelegate = Delegate.CreateDelegate(scriptExecutionStarted.EventHandlerType, eventHandler.Target, eventHandler.Method);
                        scriptExecutionStarted.RemoveEventHandler(sqlResultsControl, handlerDelegate);
                        scriptExecutionStarted.AddEventHandler(sqlResultsControl, handlerDelegate);
                    }
                }
            }
        }

        public static void UpdateStatusStripBackColorEventHandler(object QEOLESQLExec, object b)
        {
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            if (sqlScriptEditorControl != null)
            {
                var statusBarManager = sqlScriptEditorControl.StatusBarManager;
                if (statusBarManager != null)
                {
                    if (Reflection.GetField(statusBarManager, "statusStrip") is StatusStrip statusStrip)
                    {
                        var htmlColor = GetColorSetting();
                        var settingColor = ColorTranslator.FromHtml(htmlColor);
                        if (settingColor != statusStrip.BackColor)
                            statusStrip.BackColor = ColorTranslator.FromHtml(htmlColor);
                    }
                }
            }
        }

        private static void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (GotFocus.Object is SqlScriptEditorControl)
                SetColorStatusStrip();
        }

        private static void SetColorStatusStrip()
        {
            var htmlColor = GetColorSetting();
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            if (sqlScriptEditorControl != null)
            {
                var statusBarManager = sqlScriptEditorControl.StatusBarManager;
                if (statusBarManager != null)
                {
                    _statusStripCached = Reflection.GetField(statusBarManager, "statusStrip") as StatusStrip;
                    if (_statusStripCached != null)
                        _statusStripCached.LayoutCompleted += (s, e) => StatusStrip_LayoutCompleted(s, ColorTranslator.FromHtml(htmlColor), e);
                }
            }
        }

        private static void StatusStrip_LayoutCompleted(object statusStripSender, Color colorValue, EventArgs e)
        {
            if (statusStripSender is StatusStrip statusStrip)
            {
                if (statusStrip.BackColor != colorValue)
                    statusStrip.BackColor = colorValue;
            }
        }

        private static string GetColorSetting()
        {
            var connection = _sqlConnection.GetCurrentSqlConnection();
            var options = _sqlAsyncPackage.Options;
            var alertColorOptions = options.AlertColors.FirstOrDefault(opt => opt.ServerName == connection.ServerName && (opt.Database == "." || opt.Database == connection.Database));
            return alertColorOptions != null ? alertColorOptions.ColorHex : YELLOW_COLOR;
        }
    }
}