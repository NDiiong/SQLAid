using EnvDTE;

namespace SQLAid.Integration
{
    public interface ITextDocumentService
    {
        Document GetActiveDocument();

        TextDocument GetTextDocument();

        TextSelection GetTextSelection();
    }
}