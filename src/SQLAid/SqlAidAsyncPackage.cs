using Microsoft.VisualStudio.Shell;
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
            //SQL EVENTS
            //await WindowEventLogging.InitializeAsync(this).ConfigureAwait(false);
            //await QueryHistoryCommand.InitializeAsync(this).ConfigureAwait(false);

            //SQL GRID RESULT
            await SqlResultGridCopyAsInsertCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsCsharpModelCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsSeedDataCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsJsonCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridCopyAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);

            await SqlResultGridAsSaveCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridFrozenColumnCommand.InitializeAsync(this).ConfigureAwait(false);

            //SQL TEXT EDITOR
            await SqlJoinLinesCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);
            //await SqlPasteAsInsertedQueryCommand.InitializeAsync(this).ConfigureAwait(false);
            //await SqlPasteAsInsertedHeaderQueryCommand.InitializeAsync(this).ConfigureAwait(false);
        }
    }
}