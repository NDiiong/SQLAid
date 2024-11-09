using EnvDTE;

namespace SQLAid.Integration.DTE
{
    public class EditorService : IEditorService
    {
        public void FormatLine(TextSelection selection, EditorPosition position)
        {
            selection.MoveToLineAndOffset(position.Line, position.Column);
            selection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
            selection.EndOfLine(true);
            selection.MoveToLineAndOffset(position.Line, position.Column);
            selection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
        }

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

        public void ReplaceSelection(TextSelection selection, string newText)
        {
            selection.Delete(1);
            selection.Collapse();
            var editPoint = selection.TopPoint.CreateEditPoint();
            editPoint.Insert(newText);
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