using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Addin.Extension;
using SQLAid.Helpers;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands
{
    internal sealed class SqlPasteAsInsertedQueryCommand
    {
        private static readonly IClipboardService _clipboardService;
        private static readonly ITextDocumentService _textDocumentService;

        static SqlPasteAsInsertedQueryCommand()
        {
            _clipboardService = new ClipboardService();
            _textDocumentService = new TextDocumentService();
        }

        public static async Task InitializeAsync(Package package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsInsertedCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(dte), cmdId);
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

        private static void Execute(DTE2 dte)
        {
            try
            {
                var content = _clipboardService.GetFromClipboard();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var numberOfTab = content
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ElementAtOrDefault(0)
                        ?.Count(q => q == '\t');

                    if (numberOfTab != null)
                    {
                        var columnArray = new string[numberOfTab.Value + 1];
                        var columns = string.Join(", ", columnArray.Select((_, index) => "column" + index));

                        var editor = new Editor(_textDocumentService.GetTextDocument());
                        content = editor.Sanitize(content, columns);

                        var undoTransaction = new UndoTransaction(dte, "SqlPasteAsInsertedQuery");
                        undoTransaction.Run(() => editor.SetContent(content, count: 1));
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}