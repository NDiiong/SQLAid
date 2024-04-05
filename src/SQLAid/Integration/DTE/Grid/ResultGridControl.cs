using Microsoft.SqlServer.Management.QueryExecution;
using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SQLAid.Extensions;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControl : IResultGridControl
    {
        public void ChangeWindowTitle(string text)
        {
            var gridControl = GetFocusGridControl();

            var m_rawSP = gridControl.GetField("m_rawSP");
            var frame = m_rawSP.GetField("frame");
            var doc = frame.GetProperty("DocumentObject").GetType().GetProperty("DocumentObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(frame);
            var windowPane = doc.GetPropertyValue("WindowPane");
            var statusBarManager = windowPane.GetField("statusBarManager");

            var generalPanel = statusBarManager.GetField("generalPanel");
            var generalPanel_ForeColorProperty = generalPanel.GetType().GetProperty("ForeColor");
            generalPanel_ForeColorProperty.SetValue(generalPanel, Color.Red);

            Font defaultFont = new Font("Segoe UI", 9);
            Font boldFont = new Font("Segoe UI", 10, FontStyle.Bold);

            var statusStrip = statusBarManager.GetField("statusStrip");

            var statusStrip_FontProperty = statusStrip.GetType().GetProperty("Font");
            statusStrip_FontProperty.SetValue(statusStrip, boldFont);
        }

        public object GetGridControl()
        {
            var scriptFactory = ServiceCache.ScriptFactory.GetType();
            var methodGetCurrentlyActiveFrameDocView = scriptFactory.GetMethod("GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = methodGetCurrentlyActiveFrameDocView.Invoke(ServiceCache.ScriptFactory, new object[] { ServiceCache.VSMonitorSelection, false, null });

            var @object = result.GetType();
            var field = @object.GetField("m_sqlResultsControl", BindingFlags.NonPublic | BindingFlags.Instance);
            var resultsControl = field.GetValue(result);

            return resultsControl;
        }

        public string GetQueryText()
        {
            var textSpan = ServiceCache.ScriptFactory.InvokeMethod<ITextSpan>("GetSelectedTextSpan");
            return textSpan.Text;
        }

        public CollectionBase GetGridContainers()
        {
            var resultsControl = GetGridControl();
            var gridResultsPage = Reflection.GetField(resultsControl, "m_gridResultsPage");
            var gridContainers = Reflection.GetField(gridResultsPage, "m_gridContainers") as CollectionBase;
            return gridContainers;
        }

        public IGridControl GetFocusGridControl()
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