using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Addin.Extension;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControl : ResultGridControlBase
    {
        public override IGridControl GetCurrentGridControl()
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