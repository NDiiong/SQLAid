using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Options;
using System.ComponentModel.Design;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class OpenLogDirectoryCommand
    {
        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.OpenLogCommand);
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
            Process.Start("explorer.exe", SQLAidOptions.LogDirectory);
        }
    }
}