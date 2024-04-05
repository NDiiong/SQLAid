using Microsoft.SqlServer.Management.QueryExecution;
using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using SQLAid.Extensions;
using System.Collections;

namespace SQLAid.Integration.DTE.Grid
{
    public abstract class ResultGridControlBase : IResultGridControl
    {
        public abstract object GetGridControl();

        public abstract CollectionBase GetGridContainers();

        public string GetQueryText()
        {
            var textSpan = ServiceCache.ScriptFactory.InvokeMethod<ITextSpan>("GetSelectedTextSpan");
            return textSpan.Text;
        }

        public abstract IGridControl GetFocusGridControl();
    }
}