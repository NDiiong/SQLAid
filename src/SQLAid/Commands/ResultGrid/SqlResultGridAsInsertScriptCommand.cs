#pragma warning disable IDE1006

using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Commandbars;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Logging;
using System;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridAsInsertScriptCommand : SqlResultGridCommandBase
    {
        private static readonly IFrameDocumentView _frameDocumentView;

        static SqlResultGridAsInsertScriptCommand()
        {
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAidAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var generateScriptCommbarPopup = SqlAidGridControl
                 .As<CommandBarPopup>().Controls
                 .Add(MsoControlType.msoControlPopup, Type.Missing, Type.Missing, Type.Missing, true)
                 .Visible(true)
                 .Caption("Generate Script As...")
                 .As<CommandBarPopup>();

            generateScriptCommbarPopup.Controls
                .Add(MsoControlType.msoControlButton, 1, Type.Missing, Type.Missing, false)
                .Visible(true)
                .Caption("INSERT")
                .As<CommandBarButton>()
                .Click += (CommandBarButton _, ref bool __) => Execute(package);
        }

        private static void Execute(SqlAidAsyncPackage asyncPackage)
        {
            Func.Run(() =>
            {
                var currentGridControl = GridControl.GetCurrentGridControl();
                if (currentGridControl != null)
                {
                    using (var gridResultControl = new ResultGridControlAdaptor(currentGridControl))
                    {
                        var templates = File.ReadAllText($"{asyncPackage.ExtensionInstallationDirectory}/Templates/SQL.INSERT.INTO.sql");

                        var gridResultSelected = gridResultControl.GridSelectedAsQuerySql();
                        var columnHeaders = gridResultSelected.ElementAt(0);
                        var contentRows = gridResultSelected.Skip(1);

                        var rows = string.Join("\r\n\t,", contentRows.Select(r => $"({string.Join(", ", r)})"));
                        var sqlQuery = templates.Replace("{columnHeaders}", columnHeaders).Replace("{rows}", rows);

                        var textDocument = _frameDocumentView.GetTextDocument();
                        textDocument.EndPoint.CreateEditPoint().Insert(sqlQuery);
                    }
                }
            });
        }
    }
}