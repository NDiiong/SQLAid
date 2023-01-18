using Microsoft.VisualStudio.Shell;
using SQLAid.Services.SqlPackage;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Services.Command
{
    internal abstract class CommandBase<T> where T : CommandBase<T>, new()
    {
        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var command = (CommandBase<T>)new T();
            await command.InitializeCommandAsync(package);
        }

        protected abstract Task InitializeCommandAsync(SqlAsyncPackage package);
    }
}