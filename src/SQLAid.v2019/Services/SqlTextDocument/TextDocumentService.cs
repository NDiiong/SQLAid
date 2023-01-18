using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace SQLAid.Services.SqlTextDocument
{
    public class TextDocumentService : ITextDocumentService
    {
        public void Collapse(string text)
        {
            var textDocument = GetTextDocument();
            var textSelection = textDocument.Selection;
            var currentline = textSelection.TopPoint.Line;
            var currentColumn = textSelection.TopPoint.DisplayColumn;

            textSelection.Delete(1);
            textSelection.Collapse();
            textSelection.MoveToLineAndOffset(currentline, currentColumn);

            var ed = textSelection.TopPoint.CreateEditPoint();
            ed.Insert(text);

            textSelection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
            textSelection.EndOfLine(true);

            textSelection.MoveToLineAndOffset(currentline, currentColumn);
        }

        public Document GetActiveDocument()
        {
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
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