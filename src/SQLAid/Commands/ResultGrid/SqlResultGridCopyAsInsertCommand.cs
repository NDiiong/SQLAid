#pragma warning disable IDE1006

using Microsoft.CSharp;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using System.CodeDom;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsCsharpModelCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyAsCsharpModelCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GridCommandBar.AddButton("Copy As C# Model", OnClick);
        }

        private static void OnClick()
        {
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var schema = gridResultControl.SchemaResultGrid();
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine();
                sb.AppendLine($"public class Root");
                sb.AppendLine("{");

                foreach (DataColumn column in schema.Columns)
                {
                    var compiler = new CSharpCodeProvider();
                    var type = new CodeTypeReference(column.DataType);
                    sb.AppendLine($"    public {compiler.GetTypeOutput(type)} {column.ColumnName} {{ get; set; }}");
                }

                sb.AppendLine("}");
                _clipboardService.Set(sb.ToString());
            }
        }
    }

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
            templates = File.ReadAllText($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/SQL.INSERT.INTO.sql");
            GridCommandBar.AddButton("Copy As #INSERT", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var gridResultSelected = gridResultControl.GridSelectedAsQuerySql();
                var columnHeaders = gridResultSelected.ElementAt(0);
                var contentRows = gridResultSelected.Skip(1);

                var rows = string.Join("\r\n\t,", contentRows.Select(r => $"({string.Join(", ", r)})"));
                var sqlQuery = templates.Replace("{columnHeaders}", columnHeaders).Replace("{rows}", rows);

                _clipboardService.Set(sqlQuery);
                ServiceCache.ExtensibilityModel.StatusBar.Text = "Copied";
            }
        }
    }
}