using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class PasteAsParenthesesCommand
    {
        private static SqlAsyncPackage _sqlAsyncPackage;
        private static readonly IClipboardService _clipboardService;
        private static readonly IFrameDocumentView _frameDocumentView;

        static PasteAsParenthesesCommand()
        {
            _clipboardService = new ClipboardService();
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _sqlAsyncPackage = sqlAsyncPackage;
            var commandService = _sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsParenthesesCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => CanExecute(s);
            commandService.AddCommand(menuItem);
        }

        private static void CanExecute(object s)
        {
            var oleMenuCommand = s as OleMenuCommand;
            if (!string.IsNullOrWhiteSpace(_clipboardService.GetFromClipboard()))
                oleMenuCommand.Visible = true;
        }

        private static void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var values = _clipboardService.GetFromClipboard();
            var valuesSplited = values.Split(',');
            if (valuesSplited.Length > 0)
            {
                var formattedNumbers = new string[valuesSplited.Length];
                for (int i = 0; i < valuesSplited.Length; i++)
                {
                    formattedNumbers[i] = $"(N'{valuesSplited[i].Trim()}')";
                }

                var result = string.Join("," + Environment.NewLine, formattedNumbers);
                var selection = _frameDocumentView.GetTextSelection();
                selection.Insert(result);
            }
        }
    }
}