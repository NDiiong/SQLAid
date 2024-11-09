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
    internal sealed class SqlResultGridCopyTextTableFormatterCommand : SqlResultGridCommandBase
    {
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyTextTableFormatterCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GridCommandBar.AddButton("Copy As 'Text Table' Formatting", OnClick);
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