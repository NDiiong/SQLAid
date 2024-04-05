#pragma warning disable IDE1006

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration;
using SQLAid.Integration.DTE.Grid;

namespace SQLAid.Commands.ResultGrid
{
    internal abstract class SqlResultGridCommandBase
    {
        private const string SQL_RESULT_GRID_CONTEXT_NAME = "SQL Results Grid Tab Context";
        protected static CommandBar GridCommandBar { get; }

        protected static readonly IResultGridControl GridControl;

        static SqlResultGridCommandBase()
        {
            GridControl = new ResultGridControl();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            GridCommandBar = ((CommandBars)dte.CommandBars)[SQL_RESULT_GRID_CONTEXT_NAME];
        }
    }
}