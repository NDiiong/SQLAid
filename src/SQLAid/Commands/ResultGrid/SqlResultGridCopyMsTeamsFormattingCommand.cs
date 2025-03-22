#pragma warning disable IDE1006

using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyMsTeamsFormattingCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;
        private static SqlAsyncPackage _sqlAsyncPackage;

        static SqlResultGridCopyMsTeamsFormattingCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _sqlAsyncPackage = sqlAsyncPackage;

            GridCommandBar.AddButton("Copy As 'MS Teams' Formatting", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/copy-special.ico", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var htmlTemplates = File.ReadAllText($"{_sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/Internal/MSTeamMessageChatsFormat.html");
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var datatable = gridResultControl.GetSelectedCellsAsDataTable();
                var tableHtml = datatable.ToTeamsFormat();
                var content = htmlTemplates.Replace("{table}", tableHtml);
                _clipboardService.Set(content, TextDataFormat.Html);
            }
        }
    }
}