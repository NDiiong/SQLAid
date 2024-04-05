using EnvDTE;

namespace SQLAid.Integration.DTE
{
    public class FrameDocumentView : IFrameDocumentView
    {
        //public Document GetActiveDocument()
        //{
        //    return ServiceCache.ExtensibilityModel.ActiveDocument.DTE?.ActiveDocument;
        //}

        //public SqlScriptEditorControl GetCurrentlyActiveFrameDocView()
        //{
        //    return ServiceCache.ScriptFactory
        //        .InvokeMethod<SqlScriptEditorControl>("GetCurrentlyActiveFrameDocView", new object[] { ServiceCache.VSMonitorSelection, false, null });
        //}

        //public TextDocument GetTextDocument()
        //{
        //    return (TextDocument)GetActiveDocument().Object("TextDocument");
        //}

        //public TextSelection GetTextSelection()
        //{
        //    return GetTextDocument().Selection;
        //}
        public Document GetActiveDocument()
        {
            throw new System.NotImplementedException();
        }

        public TextDocument GetTextDocument()
        {
            throw new System.NotImplementedException();
        }

        public TextSelection GetTextSelection()
        {
            throw new System.NotImplementedException();
        }
    }
}