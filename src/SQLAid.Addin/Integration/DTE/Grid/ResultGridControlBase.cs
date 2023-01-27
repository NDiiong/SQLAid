using Microsoft.SqlServer.Management.QueryExecution;
using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using SQLAid.Addin.Extension;

namespace SQLAid.Integration.DTE.Grid
{
    public abstract class ResultGridControlBase : IResultGridControl
    {
        public abstract IGridControl GetCurrentGridControl();

        public string GetQueryText()
        {
            var textSpan = ServiceCache.ScriptFactory.InvokeMethod<ITextSpan>("GetSelectedTextSpan");
            return textSpan.Text;
        }
    }
}