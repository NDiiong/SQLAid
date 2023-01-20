using Microsoft.VisualStudio.Shell;
using SQLAid.Commands;
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
        public SqlAidAsyncPackage() : base(PackageGuids.guidSQLAidPackageString)
        {
        }

        protected override async Task InitializeAsync()
        {
            //await WindowEventLogging.InitializeAsync(this).ConfigureAwait(false);
            await QueryHistoryCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlJoinLinesCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlCopyAsGridResultCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlSaveAsGridResultCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsInsertedQueryCommand.InitializeAsync(this).ConfigureAwait(false);
            //await SqlInsertScriptGridResultCommand.InitializeAsync(this).ConfigureAwait(false);
        }
    }

    /*
     * BUILDING
     * SSMS VERSION    VS VESION     VS BUILD
     *  19.0
     *                   2019       15.0
     *                   2017       14.0
     *  17.0             2015
     */
}