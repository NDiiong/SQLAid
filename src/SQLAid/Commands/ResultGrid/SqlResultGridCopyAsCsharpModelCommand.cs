#pragma warning disable IDE1006

using Microsoft.CSharp;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
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
                    //Primitive Type
                    var compiler = new CSharpCodeProvider();
                    var type = new CodeTypeReference(column.DataType);
                    sb.AppendLine($"    public {compiler.GetTypeOutput(type)} {column.ColumnName} {{ get; set; }}");
                }

                sb.AppendLine("}");
                _clipboardService.Set(sb.ToString());
            }
        }
    }
}