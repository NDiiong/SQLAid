using EnvDTE;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.UndoTransaction;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlJoinLinesCommand
    {
        private readonly IFrameDocumentView _documentView;
        private readonly ILineJoiner _lineJoiner;
        private readonly IUndoTransactionFactory _undoFactory;
        private readonly IEditorService _editorService;

        public SqlJoinLinesCommand(
            IFrameDocumentView documentView,
            ILineJoiner lineJoiner,
            IUndoTransactionFactory undoFactory,
            IEditorService editorService)
        {
            _documentView = documentView ?? throw new ArgumentNullException(nameof(documentView));
            _lineJoiner = lineJoiner ?? throw new ArgumentNullException(nameof(lineJoiner));
            _undoFactory = undoFactory ?? throw new ArgumentNullException(nameof(undoFactory));
            _editorService = editorService ?? throw new ArgumentNullException(nameof(editorService));
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var command = CreateCommand(package);
            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            commandService.AddCommand(command);
        }

        private static OleMenuCommand CreateCommand(SqlAsyncPackage package)
        {
            var command = new SqlJoinLinesCommand(
                new FrameDocumentView(),
                new LineJoiner(),
                new UndoTransactionFactory(package.Application),
                new EditorService());

            var menuCommandID = new CommandID(PackageGuids.guidCommands, PackageIds.JoinLinesCommand);
            var menuItem = new OleMenuCommand((s, e) => command.Execute(), menuCommandID);
            menuItem.BeforeQueryStatus += (s, e) => command.UpdateCommandStatus(s);

            return menuItem;
        }

        private void UpdateCommandStatus(object sender)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is OleMenuCommand commandMenu)
            {
                try
                {
                    commandMenu.Visible = HasSelectedText();
                }
                catch (Exception)
                {
                    commandMenu.Visible = false;
                }
            }
        }

        private bool HasSelectedText()
        {
            var textDocument = _documentView.GetTextDocument();
            return textDocument?.Selection != null && !string.IsNullOrEmpty(textDocument.Selection.Text);
        }

        private void Execute()
        {
            var textDocument = _documentView.GetTextDocument();
            if (textDocument == null)
                return;

            using (var undoTransaction = _undoFactory.Create(nameof(SqlJoinLinesCommand)))
            {
                JoinSelectedLines(textDocument.Selection);
            }
        }

        private void JoinSelectedLines(TextSelection selection)
        {
            if (string.IsNullOrEmpty(selection.Text)) return;

            var cursorPosition = _editorService.GetCursorPosition(selection);
            var joinedText = _lineJoiner.Join(selection.Text);

            _editorService.ReplaceSelection(selection, joinedText);
            _editorService.FormatLine(selection, cursorPosition);
        }
    }

    public interface ILineJoiner
    {
        string Join(string content);
    }

    public class LineJoiner : ILineJoiner
    {
        private const string Pattern = @"[ \t]*\r?\n[ \t]*";

        public string Join(string content)
        {
            return Regex.Replace(content, Pattern, " ", RegexOptions.Multiline);
        }
    }
}