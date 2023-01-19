using Microsoft.VisualStudio.Shell;
using SQLAid.Services.Extension;
using SQLAid.Services.Runtime;
using SQLAid.Services.SqlTextDocument;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands
{
    internal sealed class SqlPasteAsCsvCommand
    {
        private const string PATTERN = @"[ \t]*\r?\n[ \t]*";
        private static readonly ITextDocumentService _textDocumentService;
        private static readonly IClipboardService _clipboardService;

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
                    string replacement = "'," + Environment.NewLine + "'";
                    var singleLine = $"'{Regex.Replace(content, PATTERN, replacement, RegexOptions.Multiline)}'";

                    _textDocumentService.Collapse(singleLine);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}