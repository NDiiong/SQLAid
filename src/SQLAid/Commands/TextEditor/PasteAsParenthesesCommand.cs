using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
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

        public PasteAsParenthesesCommand(
            IClipboardService clipboardService,
            IFrameDocumentView frameDocumentView)
        {
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _frameDocumentView = frameDocumentView ?? throw new ArgumentNullException(nameof(frameDocumentView));
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var command = CreateCommand();
            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            commandService.AddCommand(command);
        }

        private static OleMenuCommand CreateCommand()
        {
            var command = new PasteAsParenthesesCommand(new ClipboardService(), new FrameDocumentView());

            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsParenthesesCommand);
            var menuItem = new OleMenuCommand((s, e) => command.Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => command.UpdateCommandStatus(s);

            return menuItem;
        }

        private void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var clipboardContent = _clipboardService.GetFromClipboard();
            if (string.IsNullOrWhiteSpace(clipboardContent))
                return;

            var formattedValues = FormatClipboardContent(clipboardContent);
            InsertFormattedValues(formattedValues);
        }

        private void UpdateCommandStatus(object sender)
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