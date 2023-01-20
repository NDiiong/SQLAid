using Microsoft.VisualStudio.Shell;
using SQLAid.Addin.Extension;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands
{
    internal sealed class SqlPasteAsCsvCommand
    {
        private const string PATTERN = @"[ \t]*\r?\n[ \t]*";
        private static readonly IClipboardService _clipboardService;
        private static readonly ITextDocumentService _textDocumentService;

        static SqlPasteAsCsvCommand()
        {
            _textDocumentService = new TextDocumentService();
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(Package package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();

            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsCsvCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => CanExecute(s);
            commandService.AddCommand(menuItem);
        }

        private static void CanExecute(object s)
        {
            try
            {
                var oleMenuCommand = s as OleMenuCommand;

                if (!string.IsNullOrWhiteSpace(_clipboardService.GetFromClipboard()))
                    oleMenuCommand.Visible = true;
            }
            catch (Exception)
            {
            }
        }

        private static void Execute()
        {
            try
            {
                var content = _clipboardService.GetFromClipboard();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var replacement = "'," + Environment.NewLine + "'";
                    var singleLine = $"'{Regex.Replace(content, PATTERN, replacement, RegexOptions.Multiline)}'";
                    var editor = new Editor(_textDocumentService.GetTextDocument());
                    editor.SetContent(singleLine, count: 1);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}