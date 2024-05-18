using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class OpenFolderTemplatesCommand
    {
        private static SqlAsyncPackage _sqlAsyncPackage;

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _sqlAsyncPackage = sqlAsyncPackage;
            var commandService = _sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.OpenFolderTemplateCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => CanExecute(s);
            commandService.AddCommand(menuItem);
        }

        private static void CanExecute(object s)
        {
            var oleMenuCommand = s as OleMenuCommand;
            oleMenuCommand.Visible = true;
        }

        private static void Execute()
        {
            System.Diagnostics.Process.Start("explorer.exe", _sqlAsyncPackage.Options.TemplateDirectory);
        }
    }
}