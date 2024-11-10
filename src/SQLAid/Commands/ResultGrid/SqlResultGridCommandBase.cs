#pragma warning disable IDE1006

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;

namespace SQLAid.Commands.ResultGrid
{
    internal abstract class SqlResultGridCommandBase
    {
        protected static CommandBar GridCommandBar { get; }
        private const string SQL_RESULT_GRID_CONTEXT_NAME = "SQL Results Grid Tab Context";

        static SqlResultGridCommandBase()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            GridCommandBar = ((CommandBars)dte.CommandBars)[SQL_RESULT_GRID_CONTEXT_NAME];
        }
    }
}