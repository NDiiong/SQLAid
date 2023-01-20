using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Addin.Extension;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControl : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            SqlServiceCache.VsMonitorSelection.GetCurrentElementValue((int)VSConstants.VSSELELEMID.SEID_WindowFrame, out var outVsWindowFrame);

            var vsWindowFrame = outVsWindowFrame as IVsWindowFrame;
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var outControl);

            var control = outControl.As<Control>();
            return control.As<ContainerControl>()
                .ActiveControl.As<ContainerControl>()
                .ActiveControl.As<GridControl>();
        }
    }
}