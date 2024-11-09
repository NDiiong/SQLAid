using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorSnippetCommand
    {
        private static TextDocumentKeyPressEvents _textDocumentKeyPressEvents;

        private static SqlAsyncPackage _sqlAsyncPackage;

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _sqlAsyncPackage = sqlAsyncPackage;
            var events = sqlAsyncPackage.Application.Events as Events2;
            _textDocumentKeyPressEvents = events.TextDocumentKeyPressEvents;
            _textDocumentKeyPressEvents.BeforeKeyPress += TextDocumentKeyPressEvents_BeforeKeyPress;
        }

        private static void TextDocumentKeyPressEvents_BeforeKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Keypress != "\t")
                return;

            var startPoint = Selection.ActivePoint.CreateEditPoint();
            startPoint.StartOfLine();

            while (!startPoint.AtEndOfLine && (startPoint.GetText(1) == "\t" || startPoint.GetText(1) == " "))
                startPoint.CharRight();

            CancelKeypress = CreateSnippet(Selection, startPoint);
        }

        private static bool CreateSnippet(TextSelection Selection, EditPoint startPoint)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var endPoint = startPoint.CreateEditPoint();
            endPoint.EndOfLine();

            var lineText = startPoint.GetText(endPoint.AbsoluteCharOffset - startPoint.AbsoluteCharOffset).TrimEnd();
            if (string.IsNullOrWhiteSpace(lineText))
                return false;

            var defaultTemplates = Directory.GetFiles($"{_sqlAsyncPackage.ExtensionInstallationDirectory}/Templates");
            var customTemplates = Directory.GetFiles(_sqlAsyncPackage.Options.TemplateDirectory).ToList();
            customTemplates.AddRange(defaultTemplates);

            foreach (var template in customTemplates)
            {
                var filename = Path.GetFileNameWithoutExtension(template);

                // Compare the entire line text with the filename
                if (lineText.Equals(filename, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    var content = File.ReadAllText(template);

                    // Delete the entire line content
                    startPoint.Delete(lineText.Length);

                    // Insert the template content
                    startPoint.Insert(content);
                    Selection.MoveToPoint(startPoint);
                    return true;
                }
            }

            return false;
        }
    }
}