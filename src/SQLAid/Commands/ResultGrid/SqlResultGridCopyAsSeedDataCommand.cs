#pragma warning disable IDE1006

using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Windows;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsSeedDataCommand : SqlResultGridCommandBase
    {
        private static string templates;
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyAsSeedDataCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            templates = File.ReadAllText($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/Internal/SQL.SEED.DATA.sql");
            GridCommandBar.AddButton("Copy As Seeding Data", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/seeding.ico", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var dataTable = gridResultControl.GridFocusAsDatatable();
                var tableNameForm = new SeedingDataForm(_clipboardService, templates, dataTable);
                tableNameForm.Show();
            }
        }
    }
}