using EnvDTE;

namespace SQLAid.Services.SqlTextDocument
{
    public interface ITextDocumentService
    {
        void Collapse(string text);

        Document GetActiveDocument();

        TextDocument GetTextDocument();

        TextSelection GetTextSelection();
    }
}