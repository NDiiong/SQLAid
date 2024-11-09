using EnvDTE;
using SQLAid.Integration.DTE;

namespace SQLAid.Integration
{
    public interface IEditorService
    {
        EditorPosition GetCursorPosition(TextSelection textSelection);

        void InsertText(TextSelection textSelection, string text);

        void RestoreCursorPosition(TextSelection textSelection, EditorPosition position);
    }
}