#pragma warning disable IDE1006

using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Integration.DTE.Grid.Result;
using SQLAid.Templates;
using System;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsInsertCommand : SqlResultGridCommandBase
    {
        private readonly IGridComponent _gridComponent;
        private readonly IGridComponentAdaptor _gridComponentAdaptor;
        private readonly IClipboardService _clipboardService;
        private readonly ITemplateProvider _templateProvider;

        public SqlResultGridCopyAsInsertCommand(
            IGridComponent gridComponent,
            IGridComponentAdaptor gridComponentAdaptor,
            IClipboardService clipboardService,
            ITemplateProvider templateProvider)
        {
            _gridComponent = gridComponent;
            _gridComponentAdaptor = gridComponentAdaptor;
            _clipboardService = clipboardService;
            _templateProvider = templateProvider;
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var command = new SqlResultGridCopyAsInsertCommand(
                new GridComponent(),
                new GridComponentAdaptor(),
                new ClipboardService(),
                new TemplateProvider(sqlAsyncPackage.ExtensionInstallationDirectory));

            GridCommandBar.AddButton("Copy As #INSERT", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/insert-table.ico", createNewGroup: true, command.OnClick);
        }

        private void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var processor = _gridComponentAdaptor.GetGridSelectedProcessor(_gridComponent);
            var schema = processor.GetSchema();
            var columnHeaders = schema.GetColumnHeaders();
            var columnsbrackets = columnHeaders.Select(column => column.StartsWith("[") ? column : "[" + column + "]");

            var queryText = _templateProvider.GetTemplate("SQL.INSERT.INTO.sql");
            queryText = queryText.Replace("{columnHeaders}", string.Join(", ", columnsbrackets));

            //queryText.

            //using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            //{
            //    var gridResultSelected = gridResultControl.GridSelectedAsQuerySql();
            //    var columnHeaders = gridResultSelected.ElementAt(0);
            //    var contentRows = gridResultSelected.Skip(1);

            //    var rows = string.Join($",{Environment.NewLine}\t", contentRows.Select(r => $"({string.Join(", ", r)})"));
            //    var sqlQuery = templates.Replace("{columnHeaders}", columnHeaders).Replace("{rows}", rows);

            //    _clipboardService.Set(sqlQuery);
            //    ServiceCache.ExtensibilityModel.StatusBar.Text = "Copied";
            //}

            //_gridComponentAdaptor.Dispose();
        }
    }
}