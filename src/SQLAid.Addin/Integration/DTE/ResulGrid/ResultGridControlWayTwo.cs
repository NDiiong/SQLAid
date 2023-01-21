using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Reflection;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControlWayTwo : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            var scriptFactor = ServiceCache.ScriptFactory;
            var monitorSelection = ServiceCache.VSMonitorSelection;

            var sqlScriptEditorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            var resultControlField = sqlScriptEditorControl.GetType()
                .GetField("m_sqlResultsControl", bindingFlags);

            if (resultControlField == null)
                return null;

            var resultsControl = resultControlField.GetValue(sqlScriptEditorControl);

            var resultsTabPage = resultsControl
                .GetType()
                .GetField("m_gridResultsPage", bindingFlags)
                .GetValue(resultsControl);

            var grid = (IGridControl)resultsTabPage
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