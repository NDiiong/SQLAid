#pragma warning disable IDE1006

using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultMessages
{
    internal class SqlResultMessagesStatisticsParserCommand : SqlResultMessagesCommandBase
    {
        private static readonly IClipboardService _clipboardService;
        private static readonly IResultMessagesControl _resultMessageControl;

        static SqlResultMessagesStatisticsParserCommand()
        {
            _clipboardService = new ClipboardService();
            _resultMessageControl = new ResultMessagesControl();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            ResultMessageCommanBar.AddButton("Statistical Tables", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/statistics-32.ico", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var messages = _resultMessageControl.GetMessages();
        }
    }
}