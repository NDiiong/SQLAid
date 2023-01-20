using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Extensions;
using System;
using System.Reflection;
using System.Windows.Forms;
using IVsMonitorSelection = Microsoft.SqlServer.Management.UI.VSIntegration.IVsMonitorSelection;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class SqlManagementService : ISqlManagementService
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

            var grid = resultsTabPage.GetType().BaseType.GetProperty("FocusedGrid", bindingFlags).GetValue(resultsTabPage, null);
            var gridControl = (IGridControl)grid;
            //var grid = resultsTabPage.GetNonPublicProperty("FocusedGrid") as IGridControl;
            return default;
        }

        public GridControl GetCurrentGridControl(IServiceProvider serviceProvider)
        {
            SqlServiceCache.VsMonitorSelection.GetCurrentElementValue((int)VSConstants.VSSELELEMID.SEID_WindowFrame, out var _vsWindowFrame);

            var vsWindowFrame = _vsWindowFrame as IVsWindowFrame;
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var _control);
            switch (_control)
            {
                case Control control:

                    return control.As<ContainerControl>()
                        .ActiveControl.As<ContainerControl>()
                        .ActiveControl.As<GridControl>();
            }

            return null;
        }
    }
}