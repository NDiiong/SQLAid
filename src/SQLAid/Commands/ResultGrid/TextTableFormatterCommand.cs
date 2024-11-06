#pragma warning disable IDE1006

using DataTableFormatters;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class TextTableFormatterCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;

        static TextTableFormatterCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GridCommandBar.AddButton("Text Table Formatter", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var datatable = gridResultControl.GridSelectedAsDataTable();
                var content = datatable.GetStringRepresentation();
                _clipboardService.Set(content, TextDataFormat.Text);
            }
        }
    }
}