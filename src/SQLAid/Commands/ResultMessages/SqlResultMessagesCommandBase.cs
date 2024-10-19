#pragma warning disable IDE1006

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;

namespace SQLAid.Commands.ResultMessages
{
    internal abstract class SqlResultMessagesCommandBase
    {
        private const string SQL_RESULT_MESSAGES_CONTEXT_NAME = "SQL Results Messages Tab Context";

        protected static CommandBar ResultMessageCommanBar { get; }

        static SqlResultMessagesCommandBase()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            ResultMessageCommanBar = ((CommandBars)dte.CommandBars)[SQL_RESULT_MESSAGES_CONTEXT_NAME];
        }
    }
}