using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;

namespace SQLAid.Integration.DTE
{
    public class EditorPosition
    {
        public EditorPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Column { get; }
        public int Line { get; }
    }

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

        public LineInfo GetCurrentLineInfo(TextSelection selection)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var startPoint = selection.ActivePoint.CreateEditPoint();
            startPoint.StartOfLine();

            while (!startPoint.AtEndOfLine &&
                   (startPoint.GetText(1) == "\t" || startPoint.GetText(1) == " "))
            {
                startPoint.CharRight();
            }

            var endPoint = startPoint.CreateEditPoint();
            endPoint.EndOfLine();

            var text = startPoint.GetText(
                endPoint.AbsoluteCharOffset - startPoint.AbsoluteCharOffset).TrimEnd();

            return new LineInfo(text, startPoint);
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

        public void ReplaceLineWithTemplate(TextSelection selection, LineInfo lineInfo, string content)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            lineInfo.StartPoint.Delete(lineInfo.Text.Length);
            lineInfo.StartPoint.Insert(content);
            selection.MoveToPoint(lineInfo.StartPoint);
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

    public class LineInfo
    {
        public LineInfo(string text, EditPoint startPoint)
        {
            Text = text;
            StartPoint = startPoint ?? throw new ArgumentNullException(nameof(startPoint));
        }

        public EditPoint StartPoint { get; }
        public string Text { get; }
    }
}