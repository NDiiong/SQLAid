using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Services.Extension;
using System;
using System.Windows.Forms;

namespace SQLAid.Services.SqlControl
{
    public class SqlManagementService : ISqlManagementService
    {
        public GridControl GetCurrentGridControl(IServiceProvider serviceProvider)
        {
            var vsMonitorSelection = serviceProvider.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
            vsMonitorSelection.GetCurrentElementValue((int)VSConstants.VSSELELEMID.SEID_WindowFrame, out var _vsWindowFrame);

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