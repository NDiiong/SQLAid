#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.Files;
using System;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridAsSaveCommand : SqlResultGridCommandBase
    {
        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            GridCommandBar.AddButton(
                "Save Result As Json",
                $"{package.ExtensionInstallationDirectory}/Resources/Assets/json-download.ico",
                createNewGroup: true,
                () => SaveGridResult(".json", "Save Results As Json", "Json (*.json)|*.json", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));

            GridCommandBar.AddButton(
                "Save Result As Excel",
                $"{package.ExtensionInstallationDirectory}/Resources/Assets/xls-icon.ico",
                () => SaveGridResult(".xlsx", "Save Results As Excel", "Excel (*.xlsx)|*.xlsx", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
        }

        private static void SaveGridResult(string extension, string title, string filter, string initialDirectory)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            using (var saveDialog = new SaveFileDialog { FileName = "", Title = title, Filter = filter, InitialDirectory = initialDirectory, })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileService = FileServiceFactory.GetService(extension);
                    if (fileService != null)
                    {
                        var activeGridControl = GridControl.GetFocusGridControl();
                        using (var gridResultControl = new ResultGridControlAdaptor(activeGridControl))
                        {
                            fileService.WriteFile(saveDialog.FileName, gridResultControl.GridFocusAsDatatable());
                            ServiceCache.ExtensibilityModel.StatusBar.Text = "Succeeded";
                        }
                    }
                }
            }
        }
    }
}