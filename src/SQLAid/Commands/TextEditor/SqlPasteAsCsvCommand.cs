using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlPasteAsCsvCommand
    {
        private readonly IClipboardService _clipboardService;
        private readonly IFrameDocumentView _frameDocumentView;
        private readonly ICsvFormatter _csvFormatter;
        private readonly IEditorService _editorService;

        public SqlPasteAsCsvCommand(
            IClipboardService clipboardService,
            IFrameDocumentView frameDocumentView,
            ICsvFormatter csvFormatter,
            IEditorService editorService)
        {
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _frameDocumentView = frameDocumentView ?? throw new ArgumentNullException(nameof(frameDocumentView));
            _csvFormatter = csvFormatter ?? throw new ArgumentNullException(nameof(csvFormatter));
            _editorService = editorService ?? throw new ArgumentNullException(nameof(editorService));
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
            var command = new SqlPasteAsCsvCommand(new ClipboardService(), new FrameDocumentView(), new CsvFormatter(), new EditorService());

            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsCsvCommand);
            var menuItem = new OleMenuCommand((s, e) => command.Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => command.UpdateCommandStatus(s);

            return menuItem;
        }

        private void UpdateCommandStatus(object sender)
        {
            if (sender is OleMenuCommand menuCommand)
            {
                try
                {
                    menuCommand.Visible = !string.IsNullOrWhiteSpace(_clipboardService.GetFromClipboard());
                }
                catch (Exception)
                {
                    menuCommand.Visible = false;
                }
            }
        }

        private void Execute()
        {
            var content = _clipboardService.GetFromClipboard();
            if (string.IsNullOrWhiteSpace(content))
                return;

            var selection = _frameDocumentView.GetTextSelection();
            var cursorPosition = _editorService.GetCursorPosition(selection);
            var formattedContent = _csvFormatter.Format(content);

            _editorService.InsertText(selection, formattedContent);
            _editorService.RestoreCursorPosition(selection, cursorPosition);
        }
    }

    public interface ICsvFormatter
    {
        string Format(string content);
    }

    public class CsvFormatter : ICsvFormatter
    {
        private const string Pattern = @"[ \t]*\r?\n[ \t]*";

        public string Format(string content)
        {
            var replacement = "'," + Environment.NewLine + "'";
            var formattedContent = Regex.Replace(content, Pattern, replacement, RegexOptions.Multiline);
            return $"'{formattedContent}'";
        }
    }
}