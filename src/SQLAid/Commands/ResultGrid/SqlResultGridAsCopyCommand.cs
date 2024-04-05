#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Commandbars;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.Files;
using SQLAid.Logging;
using System;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridAsCopyCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridAsCopyCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var copyAsCommandBar = SqlAidGridControl
                .As<CommandBarPopup>().Controls
                .Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, Type.Missing, true)
                .Visible(true)
                .Caption("Copy Result As...")
                .As<CommandBarPopup>();

            //Save Result As Json
            copyAsCommandBar.Controls
                .Add(MsoControlType.msoControlButton, 1, Type.Missing, Type.Missing, false)
                .Visible(true)
                .Caption("JSON")
                .As<CommandBarButton>()
                .AddIcon($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Assets/json.ico")
                .Click += (CommandBarButton _, ref bool __) => CopyAsJsonGridResultEventHandler();
        }

        private static void CopyAsJsonGridResultEventHandler()
        {
            Func.Run(() =>
            {
                var currentGridControl = GridControl.GetCurrentGridControl();
                if (currentGridControl != null)
                {
                    using (var gridResultControl = new ResultGridControlAdaptor(currentGridControl))
                    {
                        var json = FileServiceFactory.JsonService.AsJson(gridResultControl.GridAsDatatable());

                        if (!string.IsNullOrEmpty(json))
                        {
                            _clipboardService.Set(json);
                            ServiceCache.ExtensibilityModel.StatusBar.Text = "Copied";
                        }
                    }
                }
            });
        }
    }
}