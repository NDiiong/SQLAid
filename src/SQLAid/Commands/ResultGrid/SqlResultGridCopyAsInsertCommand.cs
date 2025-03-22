#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using System;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsInsertCommand : SqlResultGridCommandBase
    {
        private static string templates;
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyAsInsertCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            templates = File.ReadAllText($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/Internal/SQL.INSERT.INTO.sql");
            GridCommandBar.AddButton("Copy As #INSERT", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/insert-table.ico", createNewGroup: true, OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var gridResultSelected = gridResultControl.GetSelectedCellsAsSqlQuery();
                var columnHeaders = gridResultSelected.ElementAt(0);
                var contentRows = gridResultSelected.Skip(1);

                var rows = string.Join($",{Environment.NewLine}\t", contentRows.Select(r => $"({string.Join(", ", r)})"));
                var sqlQuery = templates.Replace("{columnHeaders}", columnHeaders).Replace("{rows}", rows);

                _clipboardService.Set(sqlQuery);
                ServiceCache.ExtensibilityModel.StatusBar.Text = "Copied";
            }
        }
    }
}