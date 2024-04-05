using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using SQLAid.Extensions;

namespace SQLAid.Integration.DTE
{
    public class FrameDocumentView : IFrameDocumentView
    {
        public Document GetActiveDocument()
        {
            return ServiceCache.ExtensibilityModel.ActiveDocument.DTE?.ActiveDocument;
        }

        public SqlScriptEditorControl GetCurrentlyActiveFrameDocView()
        {
            return ServiceCache.ScriptFactory.InvokeMethod<SqlScriptEditorControl>("GetCurrentlyActiveFrameDocView", new object[] { ServiceCache.VSMonitorSelection, false, null });
        }

        public TextDocument GetTextDocument()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return (TextDocument)this.GetActiveDocument().Object("TextDocument");
        }

        public TextSelection GetTextSelection()
        {
            return GetTextDocument().Selection;
        }
    }
}