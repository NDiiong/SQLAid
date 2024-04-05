using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Helpers;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlPasteAsInsertedQueryCommand
    {
        private static readonly IClipboardService _clipboardService;
        private static readonly IFrameDocumentView _frameDocumentView;

        static SqlPasteAsInsertedQueryCommand()
        {
            _clipboardService = new ClipboardService();
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsInsertedCommand);
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
                    var numberOfTab = content
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ElementAtOrDefault(0)
                        ?.Count(q => q == '\t');

                    if (numberOfTab != null)
                    {
                        var columnArray = new string[numberOfTab.Value + 1];
                        var columns = string.Join(", ", columnArray.Select((_, index) => "column" + index));

                        var editor = new Editor(_frameDocumentView);
                        content = editor.SetContent(content, columns);

                        var undoTransaction = new UndoTransaction(ServiceCache.ExtensibilityModel, nameof(SqlPasteAsInsertedQueryCommand));
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