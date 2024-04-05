using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Extensions;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControl : ResultGridControlBase
    {
        public override object GetGridControl()
        {
            var scriptFactory = ServiceCache.ScriptFactory.GetType();
            var methodGetCurrentlyActiveFrameDocView = scriptFactory.GetMethod("GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = methodGetCurrentlyActiveFrameDocView.Invoke(ServiceCache.ScriptFactory, new object[] { ServiceCache.VSMonitorSelection, false, null });

            var @object = result.GetType();
            var field = @object.GetField("m_sqlResultsControl", BindingFlags.NonPublic | BindingFlags.Instance);
            var resultsControl = field.GetValue(result);

            return resultsControl;
        }

        public override CollectionBase GetGridContainers()
        {
            var resultsControl = GetGridControl();
            var gridResultsPage = Reflection.GetField(resultsControl, "m_gridResultsPage");
            var gridContainers = Reflection.GetField(gridResultsPage, "m_gridContainers") as CollectionBase;
            return gridContainers;
        }

        public override IGridControl GetFocusGridControl()
        {
            object outVsWindowFrame = null;
            ServiceCache.VSMonitorSelection.GetCurrentElementValue((int)VSConstants.VSSELELEMID.SEID_WindowFrame, ref outVsWindowFrame);

            var vsWindowFrame = outVsWindowFrame as IVsWindowFrame;
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var outControl);

            var control = outControl.As<Control>();
            return control.As<ContainerControl>()
                .ActiveControl.As<ContainerControl>()
                .ActiveControl.As<GridControl>();
        }
    }
}