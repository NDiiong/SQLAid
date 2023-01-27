using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Addin.Extension;
using SQLAid.Helpers;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlJoinLinesCommand
    {
        private const string _pattern = @"[ \t]*\r?\n[ \t]*";
        private static readonly IFrameDocumentView _textDocumentService;

        static SqlJoinLinesCommand()
        {
            _textDocumentService = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = package.GetService<IMenuCommandService, OleMenuCommandService>();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

            var menuCommandID = new CommandID(PackageGuids.guidCommands, PackageIds.JoinLinesCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(package, dte), menuCommandID);
            menuItem.BeforeQueryStatus += CanExcute;
            commandService.AddCommand(menuItem);
        }

        private static void CanExcute(object sender, System.EventArgs e)
        {
            try
            {
                var commandMenu = (OleMenuCommand)sender;
                commandMenu.Visible = false;

                var textDocument = _textDocumentService.GetTextDocument();
                if (textDocument != null)
                {
                    var textSelection = textDocument.Selection;
                    if (textSelection != null && !string.IsNullOrEmpty(textSelection.Text))
                    {
                        commandMenu.Visible = true;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void Execute(SqlAsyncPackage _, DTE2 dte)
        {
            try
            {
                var textDocument = _textDocumentService.GetTextDocument();

                if (textDocument != null)
                {
                    var undoTransaction = new UndoTransaction(dte, nameof(SqlJoinLinesCommand));
                    undoTransaction.Run(() => JoinLine(textDocument.Selection));
                }
            }
            catch (Exception)
            {
            }
        }

        private static void JoinLine(TextSelection textSelection)
        {
            var content = textSelection.Text;
            if (!string.IsNullOrEmpty(content))
            {
                var currentline = textSelection.TopPoint.Line;
                var currentColumn = textSelection.TopPoint.DisplayColumn;

                var singleLine = Regex.Replace(content, _pattern, " ", RegexOptions.Multiline);
                textSelection.Delete(1);
                textSelection.Collapse();
                textSelection.MoveToLineAndOffset(currentline, currentColumn);

                var ed = textSelection.TopPoint.CreateEditPoint();
                ed.Insert(singleLine);

                textSelection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
                textSelection.EndOfLine(true);
                textSelection.MoveToLineAndOffset(currentline, currentColumn);
                textSelection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
            }
        }
    }
}