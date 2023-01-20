using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using System.Reflection;
using IVsMonitorSelection = Microsoft.SqlServer.Management.UI.VSIntegration.IVsMonitorSelection;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControlWayTwo : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            IScriptFactory scriptFactor = ServiceCache.ScriptFactory;
            IVsMonitorSelection monitorSelection = ServiceCache.VSMonitorSelection;

            object editorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            FieldInfo resultControlField = editorControl.GetType()
                .GetField("m_sqlResultsControl", bindingFlags);

            if (resultControlField == null)
                return null;

            object resultsControl = resultControlField.GetValue(editorControl);

            object resultsTabPage = resultsControl
                .GetType()
                .GetField("m_gridResultsPage", bindingFlags)
                .GetValue(resultsControl);

            IGridControl grid = (IGridControl)resultsTabPage
                .GetType()
                .BaseType
                .GetProperty("FocusedGrid", bindingFlags)
                .GetValue(resultsTabPage, null);

            if (grid == null)
                return null;

            return grid;
        }
    }
}