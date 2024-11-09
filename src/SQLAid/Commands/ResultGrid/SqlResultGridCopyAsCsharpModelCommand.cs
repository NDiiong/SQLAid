#pragma warning disable IDE1006

using Microsoft.CSharp;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using System;
using System.CodeDom;
using System.Data;
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
            GridCommandBar.AddButton("Copy As C# Model", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/csharp-icon.ico", createNewGroup: true, OnClick);
        }

        private static void OnClick()
        {
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var schema = gridResultControl.SchemaResultGrid();
                var modelCode = GenerateCSharpModelCode(schema);
                _clipboardService.Set(modelCode);
            }
        }

        private static string GenerateCSharpModelCode(DataTable schema)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("public class Root");
            sb.AppendLine("{");

            foreach (DataColumn column in schema.Columns)
            {
                sb.AppendLine(GeneratePropertyCode(column));
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GeneratePropertyCode(DataColumn column)
        {
            if (column.DataType == typeof(DateTime))
                return $"    public DateTime {column.ColumnName} {{ get; set; }}";
            if (column.DataType == typeof(DateTimeOffset))
                return $"    public DateTimeOffset {column.ColumnName} {{ get; set; }}";
            if (column.DataType == typeof(Guid))
                return $"    public Guid {column.ColumnName} {{ get; set; }}";

            return GeneratePrimitivePropertyCode(column);
        }

        private static string GeneratePrimitivePropertyCode(DataColumn column)
        {
            var compiler = new CSharpCodeProvider();
            return $"    public {compiler.GetTypeOutput(new CodeTypeReference(column.DataType))} {column.ColumnName} {{ get; set; }}";
        }
    }
}