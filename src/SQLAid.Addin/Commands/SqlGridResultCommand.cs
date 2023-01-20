#pragma warning disable IDE1006

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration;
using SQLAid.Integration.DTE.SqlControl;

namespace SQLAid.Commands
{
    internal abstract class SqlGridResultCommand
    {
        private const string SQL_RESULT_GRID_CONTEXT_NAME = "SQL Results Grid Tab Context";
        protected static CommandBar SqlResultGridContext { get; }

        protected static readonly IResultGridControl GridControl;

        static SqlGridResultCommand()
        {
            GridControl = new ResultGridControl();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            SqlResultGridContext = ((CommandBars)dte.CommandBars)[SQL_RESULT_GRID_CONTEXT_NAME];
        }
    }
}