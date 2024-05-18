using Microsoft.VisualStudio.Shell;
using SQLAid.Commands.Events;
using SQLAid.Commands.ResultGrid;
using SQLAid.Commands.TextEditor;
using SQLAid.Integration.DTE;
using System;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace SQLAid
{
    [Guid(PackageGuids.guidSQLAidPackageString)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    public sealed class SqlAidAsyncPackage : SqlAsyncPackage
    {
        public const string NAME = "SQLAid";

        public SqlAidAsyncPackage() : base(PackageGuids.guidSQLAidPackageString)
        {
        }

        protected override async Task InitializeAsync()
        {
            await WindowEventLogging.InitializeAsync(this).ConfigureAwait(false);
            await QueryHistoryCommand.InitializeAsync(this).ConfigureAwait(false);
            await WindowsOpenedAlertEnvironmentEvents.InitializeAsync(this).ConfigureAwait(false);

            await TeamsChatCopyKeepFormattingCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsInsertCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsCsharpModelCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsSeedDataCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsJsonCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridAsSaveCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridFrozenColumnCommand.InitializeAsync(this).ConfigureAwait(false);

            await SqlJoinLinesCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlEditorNewGuidCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlEditorSnippetCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlEditorAutoAddBracketCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsInsertedHeaderQueryCommand.InitializeAsync(this).ConfigureAwait(false);
            await OpenQueryHistoryCommand.InitializeAsync(this).ConfigureAwait(false);
            await OpenSettingsCommand.InitializeAsync(this).ConfigureAwait(false);
            await OpenFolderTemplatesCommand.InitializeAsync(this).ConfigureAwait(false);
        }
    }
}