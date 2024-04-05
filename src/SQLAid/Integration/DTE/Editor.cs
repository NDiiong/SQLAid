using EnvDTE;
using System.Collections.Specialized;

namespace SQLAid.Integration.DTE
{
    public class Editor : IEditor
    {
        private readonly IFrameDocumentView _frameDocumentView;

        public Editor(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView;
        }

        public EditedLine GetEditedLine()
        {
            var textSelection = _frameDocumentView.GetTextSelection();
            if (!string.IsNullOrEmpty(textSelection.Text))
                return new EditedLine(textSelection.Text, 0);

            var activePoint = textSelection.ActivePoint;
            var editPoint = activePoint.CreateEditPoint();

            var line = editPoint.GetLines(activePoint.Line, activePoint.Line + 1);
            var caret = activePoint.LineCharOffset - 1;

            return new EditedLine(line, caret);
        }

        public void SetContent(string content)
        {
            var textDocument = _frameDocumentView.GetTextDocument();
            var startPoint = textDocument.CreateEditPoint(textDocument.StartPoint);
            startPoint.Delete(textDocument.EndPoint);
            startPoint.Insert(content);
        }

        public void SetContent(string text, int count)
        {
            var textSelection = _frameDocumentView.GetTextSelection();
            var currentline = textSelection.TopPoint.Line;
            var currentColumn = textSelection.TopPoint.DisplayColumn;

            textSelection.Delete(count);
            textSelection.Collapse();
            textSelection.MoveToLineAndOffset(currentline, currentColumn);

            var ed = textSelection.TopPoint.CreateEditPoint();
            ed.Insert(text);

            textSelection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText);
            textSelection.EndOfLine(true);

            textSelection.MoveToLineAndOffset(currentline, currentColumn);
        }

        //public void SetContent(string text)
        //{
        //    var textSelection = _frameDocumentView.GetTextSelection();
        //    var currentline = textSelection.TopPoint.Line;
        //    var currentColumn = textSelection.TopPoint.DisplayColumn;

        //    var ed = textSelection.TopPoint.CreateEditPoint();
        //    ed.Insert(text);

        //    // Di chuyển con trỏ về vị trí ban đầu
        //    textSelection.MoveToLineAndOffset(currentline, currentColumn);
        //}

        public string SetContent(string content, string columns)
        {
            return string.Empty;
        }

        public void SetContent(StringCollection content)
        {
            var textDocument = _frameDocumentView.GetTextDocument();
            var start = textDocument.CreateEditPoint(textDocument.StartPoint);
            start.Delete(textDocument.EndPoint);

            foreach (var part in content)
                start.Insert(part);
        }

        public string GetSqlString()
        {
            var textDocument = _frameDocumentView.GetTextDocument();
            var textSelection = _frameDocumentView.GetTextSelection();
            var textSelected = textSelection.Text;
            if (textSelected.Length == 0)
            {
                textDocument.Selection.SelectAll();
                textSelected = textDocument.Selection.Text;
            }

            return textSelected;
        }
    }
}