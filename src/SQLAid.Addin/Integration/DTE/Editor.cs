using EnvDTE;
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace SQLAid.Integration.DTE
{
    public class Editor : IEditor
    {
        private const string _template = "SELECT *\r\nFROM\r\n(\r\n\tVALUES\r\n{{_datatable}}\r\n)_tables({{_columns}})";

        private readonly TextDocument _document;

        public Editor(TextDocument textDocument)
        {
            _document = textDocument;
        }

        public EditedLine GetEditedLine()
        {
            TextSelection textSelection = _document.Selection;
            if (!String.IsNullOrEmpty(textSelection.Text))
                return new EditedLine(textSelection.Text, 0);

            VirtualPoint point = textSelection.ActivePoint;
            EditPoint editPoint = point.CreateEditPoint();

            string line = editPoint.GetLines(point.Line, point.Line + 1);
            int caret = point.LineCharOffset - 1;

            return new EditedLine(line, caret);
        }

        public void SetContent(string content)
        {
            EditPoint start = _document.CreateEditPoint(_document.StartPoint);
            start.Delete(_document.EndPoint);
            start.Insert(content);
        }

        public void SetContent(string text, int count)
        {
            var textDocument = _document;
            var textSelection = textDocument.Selection;
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

        public void SetContent(StringCollection content)
        {
            EditPoint start = _document.CreateEditPoint(_document.StartPoint);
            start.Delete(_document.EndPoint);
            foreach (string part in content)
                start.Insert(part);
        }

        public string GetSqlString()
        {
            var textSelected = _document.Selection.Text;
            if (textSelected.Length == 0)
            {
                _document.Selection.SelectAll();
                textSelected = _document.Selection.Text;
            }

            return textSelected;
        }

        public string Sanitize(string content, string columns)
        {
            var result = Regex.Replace(content, @"\t", "', '", RegexOptions.Multiline);
            var queryInsert = Regex.Replace(result, "(.*[^\r\n])", "\t('$1'),", RegexOptions.Multiline);
            queryInsert = queryInsert.TrimEnd(',');

            return _template
                .Replace("{{_datatable}}", queryInsert)
                .Replace("{{_columns}}", columns)
                .Replace("'NULL'", "NULL");
        }
    }
}