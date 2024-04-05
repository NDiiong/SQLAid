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
            //SQL EVENTS
            await WindowEventLogging.InitializeAsync(this).ConfigureAwait(false);
            await QueryHistoryCommand.InitializeAsync(this).ConfigureAwait(false);

            //SQL GRID RESULT
            await SqlResultGridFrozenColumnCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridAsCopyCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridAsSaveCommand.InitializeAsync(this).ConfigureAwait(false);

            //SQL TEXT EDITOR
            await SqlJoinLinesCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsCsvCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlPasteAsInsertedQueryCommand.InitializeAsync(this).ConfigureAwait(false);
            await SqlResultGridAsInsertScriptCommand.InitializeAsync(this).ConfigureAwait(false);
        }
    }

    /*
     * BUILDING
     * SSMS VERSION     VS DLL
     *  19.0            >= 16.0
     *  18.0            >= 15.0
     *  17.0            >= 14.0
     *
     * VS VESION        VS BUILD
     *
     *  2022            >= 17.0
     *  2019            >= 15.0
     *  2017            >= 14.0
     *
     */

    //PropertyInfo piObjectExplorerContext = contextService.GetType().GetProperty("ObjectExplorerContext", System.Reflection.BindingFlags.Public | BindingFlags.Instance);
    //Object objectExplorerContext = piObjectExplorerContext.GetValue(contextService, null);

    //EventInfo ei = objectExplorerContext.GetType().GetEvent("CurrentContextChanged", System.Reflection.BindingFlags.Public | BindingFlags.Instance);

    //Delegate del = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
    //ei.AddEventHandler(objectExplorerContext, del);
}