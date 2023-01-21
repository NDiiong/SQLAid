using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControlWayFour : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            var scriptFactor = ServiceCache.ScriptFactory;
            var monitorSelection = ServiceCache.VSMonitorSelection;

            var sqlScriptEditorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            var resultControlField = sqlScriptEditorControl
                .GetType()
                .GetField("m_sqlResultsControl", bindingFlags);

            if (resultControlField == null)
                return null;

            var resultsControl = resultControlField.GetValue(sqlScriptEditorControl);

            var resultsTabPage = resultsControl
                .GetType()
                .GetField("m_gridResultsPage", bindingFlags)
                .GetValue(resultsControl);

            var controlCollection = (Control.ControlCollection)resultsTabPage
                .GetType()
                .GetProperty("Controls", bindingFlags)
                .GetValue(resultsTabPage, null);

            return GetSpecificTypeControl<GridControl>(controlCollection);
        }

        private T GetSpecificTypeControl<T>(Control.ControlCollection controlCollection) where T : Control
        {
            T @value = null;
            var i = 0;
            for (; i < controlCollection.Count; i++)
            {
                if (@value != null)
                    break;

                try
                {
                    @value = controlCollection[i] as T;
                }
                catch (Exception)
                {
                }

                if (controlCollection[i].HasChildren && @value == null)
                    @value = GetSpecificTypeControl<T>(controlCollection[i].Controls);
            }

            return @value;
        }
    }
}