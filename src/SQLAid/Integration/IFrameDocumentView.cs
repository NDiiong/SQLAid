using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;

namespace SQLAid.Integration
{
    public interface IFrameDocumentView
    {
        Document GetActiveDocument();

        TextDocument GetTextDocument();

        TextSelection GetTextSelection();

        //SqlScriptEditorControl GetCurrentlyActiveFrameDocView();
    }
}