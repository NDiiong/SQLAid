using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class PasteAsParenthesesCommand
    {
        private readonly IClipboardService _clipboardService;
        private readonly IFrameDocumentView _frameDocumentView;
        private readonly SqlAsyncPackage _sqlAsyncPackage;
        private OleMenuCommand _menuCommand;

        public PasteAsParenthesesCommand(
            SqlAsyncPackage sqlAsyncPackage,
            IClipboardService clipboardService,
            IFrameDocumentView frameDocumentView)
        {
            _sqlAsyncPackage = sqlAsyncPackage ?? throw new ArgumentNullException(nameof(sqlAsyncPackage));
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _frameDocumentView = frameDocumentView ?? throw new ArgumentNullException(nameof(frameDocumentView));
        }

        public async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = _sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsParenthesesCommand);

            _menuCommand = new OleMenuCommand(ExecuteHandler, cmdId);
            _menuCommand.BeforeQueryStatus += HandleBeforeQueryStatus;

            commandService.AddCommand(_menuCommand);
        }

        private void ExecuteHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var clipboardContent = _clipboardService.GetFromClipboard();
            if (string.IsNullOrWhiteSpace(clipboardContent)) return;

            var formattedValues = FormatClipboardContent(clipboardContent);
            InsertFormattedValues(formattedValues);
        }

        private void HandleBeforeQueryStatus(object sender, EventArgs e)
        {
            if (sender is OleMenuCommand menuCommand)
            {
                menuCommand.Visible = !string.IsNullOrWhiteSpace(_clipboardService.GetFromClipboard());
            }
        }

        private string FormatClipboardContent(string content)
        {
            var values = content.Split(',')
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => $"(N'{v.Trim()}')").ToArray();

            return string.Join("," + Environment.NewLine, values);
        }

        private void InsertFormattedValues(string formattedContent)
        {
            var selection = _frameDocumentView.GetTextSelection();
            selection.Insert(formattedContent);
        }
    }
}