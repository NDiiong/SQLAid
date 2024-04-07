using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorNewGuidCommand
    {
        private static readonly IFrameDocumentView _frameDocumentView;

        static SqlEditorNewGuidCommand()
        {
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.NewGuidCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => CanExecute(s);
            commandService.AddCommand(menuItem);
        }

        private static void CanExecute(object s)
        {
            var oleMenuCommand = s as OleMenuCommand;
            oleMenuCommand.Visible = oleMenuCommand.Enabled = true;
        }

        private static void Execute()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var guid = Guid.NewGuid().ToString().ToUpper();
            var selection = _frameDocumentView.GetTextSelection();
            selection.Insert("\"" + guid + "\"");
        }
    }
}