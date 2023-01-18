using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Services;
using SQLAid.Services.Extension;
using SQLAid.Services.Runtime;
using SQLAid.Services.SqlQuery;
using SQLAid.Services.SqlTextDocument;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands
{
    internal sealed class SqlPasteAsInsertedHeaderQueryCommand
    {
        private static readonly ISqlQueryService _sqlQueryService;
        private static readonly IClipboardService _clipboardService;
        private static readonly ITextDocumentService _textDocumentService;

        static SqlPasteAsInsertedHeaderQueryCommand()
        {
            _sqlQueryService = new SqlQueryService();
            _clipboardService = new ClipboardService();
            _textDocumentService = new TextDocumentService();
        }

        public static async Task InitializeAsync(Package package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsInsertedWithHeaderCommand);
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
                    var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        var columns = lines.ElementAtOrDefault(0)
                        ?.Split('\t')
                        ?.Select(q => q.Contains(" ") ? $"[{q}]" : q);

                        if (columns != null)
                        {
                            content = string.Join(Environment.NewLine, lines.Skip(1));
                            var columnsJoin = string.Join(", ", columns);

                            content = _sqlQueryService.Sanitize(content, columnsJoin);

                            var undoTransaction = new UndoTransaction(dte, "SqlPasteAsInsertedHeaderQuery");
                            undoTransaction.Run(() => _textDocumentService.Collapse(content));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}