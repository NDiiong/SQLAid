#pragma warning disable IDE1006

using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyColumnNameCommand : SqlResultGridCommandBase
    {
        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        }
    }
}