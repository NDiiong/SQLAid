using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorNewGuidCommand : IDisposable
    {
        private readonly IFrameDocumentView _frameDocumentView;
        private readonly OleMenuCommand _menuCommand;

        public SqlEditorNewGuidCommand(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView ?? throw new ArgumentNullException(nameof(frameDocumentView));

            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.NewGuidCommand);
            _menuCommand = new OleMenuCommand(ExecuteHandler, cmdId);
            _menuCommand.BeforeQueryStatus += HandleBeforeQueryStatus;
        }

        /// <summary>
        /// Initializes the command within the VS package
        /// </summary>
        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var documentView = new FrameDocumentView();
            var command = new SqlEditorNewGuidCommand(documentView);

            commandService.AddCommand(command._menuCommand);
        }

        /// <summary>
        /// Handles the command execution
        /// </summary>
        private void ExecuteHandler(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var guid = GenerateFormattedGuid();
            InsertGuidAtSelection(guid);
        }

        /// <summary>
        /// Generates a new formatted GUID string
        /// </summary>
        private string GenerateFormattedGuid()
        {
            return $"'{Guid.NewGuid().ToString().ToUpper()}'";
        }

        /// <summary>
        /// Inserts the GUID at the current selection
        /// </summary>
        private void InsertGuidAtSelection(string formattedGuid)
        {
            var selection = _frameDocumentView.GetTextSelection();
            selection.Insert(formattedGuid);
        }

        /// <summary>
        /// Determines if the command can be executed
        /// </summary>
        private void HandleBeforeQueryStatus(object sender, EventArgs e)
        {
            if (_menuCommand == null) return;

            _menuCommand.Visible = true;
            _menuCommand.Enabled = true;
        }

        public void Dispose()
        {
            _menuCommand.BeforeQueryStatus -= HandleBeforeQueryStatus;
        }
    }
}