#pragma warning disable IDE1006

using EnvDTE;
using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Addin.Extension;
using SQLAid.Integration.DTE.Commandbars;
using SQLAid.Integration.DTE.Grid;
using System;
using System.Drawing;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridFrozenColumnCommand : SqlResultGridCommandBase
    {
        private static CommandBarButton _commandBarButton;
        private static int _columnIndexLastest;
        private static CommandEvents _executeEvent;

        public static async Task InitializeAsync(SqlAidAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var command = ServiceCache.ExtensibilityModel.Commands.Item("Query.Execute");
            _executeEvent = ServiceCache.ExtensibilityModel.Events.get_CommandEvents(command.Guid, command.ID);
            _executeEvent.BeforeExecute += QueryExecuteEvent_BeforeExecute;

            _commandBarButton = SqlAidGridControl.As<CommandBarPopup>().Controls
                .Add(MsoControlType.msoControlButton, 1, Type.Missing, Type.Missing, false)
                .Visible(true).Caption("Frozen")
                .As<CommandBarButton>();

            _commandBarButton
                .AddIcon($"{package.ExtensionInstallationDirectory}/Assets/snowflake.ico")
                .AddStyle(MsoButtonStyle.msoButtonIconAndCaption)
                .Click += (CommandBarButton _, ref bool __) => FrozenColumnEventHandle();
        }

        private static void QueryExecuteEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            if (_commandBarButton != null)
                _commandBarButton.Caption("Frozen");
        }

        private static void FrozenColumnEventHandle()
        {
            var currentGridControl = GridControl.GetCurrentGridControl();

            if (currentGridControl != null)
            {
                var gridControl = currentGridControl.As<GridControl>();
                using (var gridControlAdaptor = new ResultGridControlAdaptor(gridControl))
                {
                    if (gridControl.FirstScrollableColumn == 1)
                    {
                        _commandBarButton.Caption("Unfrozen");
                        gridControl.GetCurrentCell(out _, out var col);
                        if (1 <= col && (col + 1) < gridControl.GridColumnsInfo.Count)
                        {
                            _columnIndexLastest = col + 1;
                            gridControl.FirstScrollableColumn = _columnIndexLastest;
                            gridControlAdaptor.SetRangeColumnBackground(1, _columnIndexLastest, Color.PowderBlue);
                        }
                        else
                        {
                            gridControl.FirstScrollableColumn = 1;
                        }
                    }
                    else
                    {
                        gridControl.FirstScrollableColumn = 1;
                        gridControlAdaptor.SetRangeColumnBackground(1, _columnIndexLastest, Color.White);
                        _commandBarButton.Caption("Frozen");
                    }

                    gridControl.GridColumnsInfo[gridControl.FirstScrollableColumn].IsWithRightGridLine = true;
                    gridControl.UpdateGrid();
                    gridControl.Refresh();
                }
            }
        }
    }
}