using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Addin.Extension;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.SqlControl
{
    public class ResultGridControlWayThree : IResultGridControl
    {
        public IGridControl GetCurrentGridControl()
        {
            object refVsWindowFrame = null;
            ServiceCache.VSMonitorSelection.GetCurrentElementValue(2, ref refVsWindowFrame);

            var vsWindowFrame = refVsWindowFrame.As<IVsWindowFrame>();
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var outControl);

            var control = outControl.As<Control>();
            var sqlScriptEditorControl = control.As<SqlScriptEditorControl>();

            var sqlResultsControl = sqlScriptEditorControl.GetNonPublicField("m_sqlResultsControl");
            var gridResultsPage = sqlResultsControl.GetNonPublicField("m_gridResultsPage");

            var focusedGrid = gridResultsPage.GetNonPublicProperty("FocusedGrid");
            return focusedGrid.As<IGridControl>();
        }
    }
}