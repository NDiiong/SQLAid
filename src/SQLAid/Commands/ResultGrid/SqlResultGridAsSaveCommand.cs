#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Commandbars;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.Files;
using SQLAid.Logging;
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

            var saveAsCommandBarPopup = SqlAidGridControl
                .As<CommandBarPopup>().Controls
                .Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, Type.Missing, true).Visible(true)
                .Caption("Save Result As...").As<CommandBarPopup>();

            //Save Result As Json
            saveAsCommandBarPopup.Controls
                .Add(MsoControlType.msoControlButton, 1, Type.Missing, Type.Missing, false)
                .Visible(true)
                .Caption("JSON")
                .As<CommandBarButton>()
                .AddIcon($"{package.ExtensionInstallationDirectory}/Assets/json.ico")
                .Click += (CommandBarButton _, ref bool __) => SaveJsonGridResultEventHandler();

            //Save Result As Excel
            saveAsCommandBarPopup.Controls
                .Add(MsoControlType.msoControlButton, 2, Type.Missing, Type.Missing, false)
                .Visible(true)
                .Caption("EXCEL")
                .As<CommandBarButton>()
                .AddIcon($"{package.ExtensionInstallationDirectory}/Assets/excel.ico")
                .Click += (CommandBarButton _, ref bool __) => SaveExcelGridResultEventHandler();
        }

        private static void SaveExcelGridResultEventHandler()
        {
            Func.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = "",
                    Title = "Save Results As Excel",
                    Filter = "Excel (*.xlsx)|*.xlsx",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                };

                FileHandler(saveFileDialog);
            });
        }

        private static void SaveJsonGridResultEventHandler()
        {
            Func.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = "",
                    Title = "Save Results As Json",
                    Filter = "Json (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                };

                FileHandler(saveFileDialog);
            });
        }

        private static void FileHandler(SaveFileDialog saveDialog)
        {
            var diaglogResult = saveDialog.ShowDialog();
            if (diaglogResult != DialogResult.Cancel)
            {
                var extension = Path.GetExtension(saveDialog.FileName).ToLower();
                var fileservice = FileServiceFactory.GetService(extension);

                if (fileservice != null)
                {
                    var currentGridControl = GridControl.GetCurrentGridControl();
                    if (currentGridControl != null)
                    {
                        using (var gridResultControl = new ResultGridControlAdaptor(currentGridControl))
                        {
                            fileservice.WriteFile(saveDialog.FileName, gridResultControl.GridAsDatatable());
                            ServiceCache.ExtensibilityModel.StatusBar.Text = "Successed";
                        }
                    }
                }
            }
        }
    }
}