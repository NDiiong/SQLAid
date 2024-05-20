using EnvDTE;
using SQLAid.Integration.DTE;

namespace SQLAid.Integration
{
    public interface IEditorService
    {
        EditorPosition GetCursorPosition(TextSelection textSelection);

        void InsertText(TextSelection textSelection, string text);

        void RestoreCursorPosition(TextSelection textSelection, EditorPosition position);

        void ReplaceSelection(TextSelection selection, string newText);

        void FormatLine(TextSelection selection, EditorPosition position);

        LineInfo GetCurrentLineInfo(TextSelection selection);

        void ReplaceLineWithTemplate(TextSelection selection, LineInfo lineInfo, string content);
    }
}