using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SQLAid.Integration.DTE
{
    public class TextDocumentService : ITextDocumentService
    {
        public Document GetActiveDocument()
        {
            var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            return dte.ActiveDocument.DTE?.ActiveDocument;
        }

        public TextDocument GetTextDocument()
        {
            return (TextDocument)GetActiveDocument().Object("TextDocument");
        }

        public TextSelection GetTextSelection()
        {
            return GetTextDocument().Selection;
        }
    }
}