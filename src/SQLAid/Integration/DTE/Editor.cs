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