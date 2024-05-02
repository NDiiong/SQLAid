#pragma warning disable IDE1006

using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Connection;
using SQLAid.Options;
using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class WindowsOpenedAlertEnvironmentEvents
    {
        private const string YELLOW_COLOR = "#f0e68c";
        private const string RED_COLOR = "#ff8080";
        private const string GREEN_COLOR = "#80ff80";

        private static WindowEvents _windowEvents;
        private static StatusStrip _statusStrip;
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

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            _windowEvents = package.Application.Events.get_WindowEvents();
            _windowEvents.WindowActivated += WindowEvents_WindowActivated;
        }

        private static void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (GotFocus.Object is SqlScriptEditorControl)
            {
                var connection = _sqlConnection.GetCurrentSqlConnection();
                var options = SQLAidOptions.GetSettings();
                var alertColorOptions = options.AlertColors.FirstOrDefault(opt => opt.ServerName == connection.ServerName && (connection.Database == "." || opt.Database == connection.Database));
                if (alertColorOptions != null)
                    SetColor(alertColorOptions.ColorHex);
            }
        }

        private static void SetColor(string htmlColor)
        {
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            var statusBarManager = sqlScriptEditorControl.StatusBarManager;
            _statusStrip = Reflection.GetField(statusBarManager, "statusStrip") as StatusStrip;
            _statusStrip.LayoutCompleted += (s, e) => _statusStrip_LayoutCompleted(s, ColorTranslator.FromHtml(htmlColor), e);
        }

        private static void _statusStrip_LayoutCompleted(object statusStrip, Color colorValue, EventArgs e)
        {
            if (statusStrip is StatusStrip _statusStrip && _statusStrip.Items.Count > 1 && _statusStrip.Items[_statusStrip.Items.Count - 1].Text.StartsWith("0 rows"))
            {
                if (_statusStrip.BackColor != colorValue)
                    _statusStrip.BackColor = colorValue;
            }
        }
    }
}