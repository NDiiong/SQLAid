#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.Files;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsJsonCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyAsJsonCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GridCommandBar.AddButton("Copy As Json", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/json-icon.ico", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var activeGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(activeGridControl))
            {
                var json = FileServiceFactory.JsonService.AsJson(gridResultControl.GetDataTable());
                if (!string.IsNullOrEmpty(json))
                {
                    _clipboardService.Set(json);
                    ServiceCache.ExtensibilityModel.StatusBar.Text = "Copied";
                }
            }
        }
    }
}