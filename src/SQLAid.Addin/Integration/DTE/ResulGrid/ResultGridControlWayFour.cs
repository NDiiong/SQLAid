using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using System;
using System.Reflection;
using System.Windows.Forms;
using IVsMonitorSelection = Microsoft.SqlServer.Management.UI.VSIntegration.IVsMonitorSelection;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControlWayFour : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

            IScriptFactory scriptFactor = ServiceCache.ScriptFactory;
            IVsMonitorSelection monitorSelection = ServiceCache.VSMonitorSelection;

            object editorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            FieldInfo resultControlField = editorControl
                .GetType()
                .GetField("m_sqlResultsControl", bindingFlags);

            if (resultControlField == null)
                return null;

            object resultsControl = resultControlField.GetValue(editorControl);

            object resultsTabPage = resultsControl
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