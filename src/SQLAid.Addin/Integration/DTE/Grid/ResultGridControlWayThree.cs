using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Addin.Extension;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControlWayThree : ResultGridControlBase
    {
        public override IGridControl GetCurrentGridControl()
        {
            object refVsWindowFrame = null;
            ServiceCache.VSMonitorSelection.GetCurrentElementValue(2, ref refVsWindowFrame);

            var vsWindowFrame = refVsWindowFrame.As<IVsWindowFrame>();
            vsWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var outControl);

            var control = outControl.As<Control>();
            var sqlScriptEditorControl = control.As<SqlScriptEditorControl>();

            var sqlResultsControl = sqlScriptEditorControl.GetField("m_sqlResultsControl");
            var gridResultsPage = sqlResultsControl.GetField("m_gridResultsPage");

            return gridResultsPage.GetProperty<IGridControl>("FocusedGrid");
        }
    }
}