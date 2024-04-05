#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.Files;
using System;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridAsSaveCommand : SqlResultGridCommandBase
    {
        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            GridCommandBar.AddButton("Save Result As Json", $"{package.ExtensionInstallationDirectory}/Resources/Assets/json-download.ico", SaveJsonGridResultEventHandler);
            GridCommandBar.AddButton("Save Result As Excel", $"{package.ExtensionInstallationDirectory}/Resources/Assets/xls-icon.ico", SaveExcelGridResultEventHandler);
        }

        private static void SaveExcelGridResultEventHandler()
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "",
                Title = "Save Results As Excel",
                Filter = "Excel (*.xlsx)|*.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };

            FileHandler(saveFileDialog);
        }

        private static void SaveJsonGridResultEventHandler()
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "",
                Title = "Save Results As Json",
                Filter = "Json (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };

            FileHandler(saveFileDialog);
        }

        private static void FileHandler(SaveFileDialog saveDialog)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var diaglogResult = saveDialog.ShowDialog();
            if (diaglogResult != DialogResult.Cancel)
            {
                var extension = Path.GetExtension(saveDialog.FileName).ToLower();
                var fileservice = FileServiceFactory.GetService(extension);

                if (fileservice != null)
                {
                    var activeGridControl = GridControl.GetFocusGridControl();
                    using (var gridResultControl = new ResultGridControlAdaptor(activeGridControl))
                    {
                        fileservice.WriteFile(saveDialog.FileName, gridResultControl.GridFocusAsDatatable());
                        ServiceCache.ExtensibilityModel.StatusBar.Text = "Successed";
                    }
                }
            }
        }
    }
}