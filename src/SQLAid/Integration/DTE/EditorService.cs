using EnvDTE;

namespace SQLAid.Integration.DTE
{
    public class EditorService : IEditorService
    {
        public EditorPosition GetCursorPosition(TextSelection textSelection)
        {
            return new EditorPosition(
                textSelection.TopPoint.Line,
                textSelection.TopPoint.DisplayColumn);
        }

        public void InsertText(TextSelection textSelection, string text)
        {
            var editPoint = textSelection.TopPoint.CreateEditPoint();
            editPoint.Insert(text);
        }

        public void RestoreCursorPosition(TextSelection textSelection, EditorPosition position)
        {
            textSelection.MoveToLineAndOffset(position.Line, position.Column);
        }
    }

    public class EditorPosition
    {
        public int Line { get; }
        public int Column { get; }

        public EditorPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }
    }
}