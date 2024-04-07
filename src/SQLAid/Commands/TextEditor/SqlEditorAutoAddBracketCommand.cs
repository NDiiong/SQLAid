using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorAutoAddBracketCommand
    {
        private static TextDocumentKeyPressEvents _textDocumentKeyPressEvents;

        static SqlEditorAutoAddBracketCommand()
        {
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var events = sqlAsyncPackage.Application.Events as Events2;
            _textDocumentKeyPressEvents = events.TextDocumentKeyPressEvents;
            _textDocumentKeyPressEvents.BeforeKeyPress += _textDocumentKeyPressEvents_BeforeKeyPress;
        }

        private static void _textDocumentKeyPressEvents_BeforeKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Keypress == "(" || Keypress == "[")
            {
                var selectedText = Selection.Text.Trim();
                if (!string.IsNullOrWhiteSpace(selectedText) && !selectedText.StartsWith(Keypress))
                {
                    var result = string.Format("{0}{1}{2}", Keypress, selectedText, ReverseCharacter(Keypress[0]));
                    Selection.Insert(result);
                    CancelKeypress = true;
                }
            }
        }

        private static char ReverseCharacter(char input)
        {
            switch (input)
            {
                case '[': return ']';
                case ']': return '[';
                case ')': return '(';
                case '(': return ')';
                default:
                    return input;
            }
        }
    }
}