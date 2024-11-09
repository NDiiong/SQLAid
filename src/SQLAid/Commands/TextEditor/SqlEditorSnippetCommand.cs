using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using System.Collections.Generic;
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

            while (!startPoint.AtEndOfLine && startPoint.GetText(1) == "\t")
                startPoint.CharRight();

            var defaultTemplates = Directory.GetFiles($"{_sqlAsyncPackage.ExtensionInstallationDirectory}/Templates");
            var customTemplates = Directory.GetFiles(_sqlAsyncPackage.Options.TemplateDirectory).ToList();
            customTemplates.AddRange(defaultTemplates);
            CancelKeypress = CreateSnippet(Selection, startPoint, customTemplates);
        }

        private static bool CreateSnippet(TextSelection Selection, EditPoint startPoint, List<string> paths)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var endPoint = startPoint.CreateEditPoint();
            endPoint.EndOfLine();

            string lineText = startPoint.GetText(endPoint.AbsoluteCharOffset - startPoint.AbsoluteCharOffset).TrimEnd();
            foreach (var path in paths)
            {
                var filename = Path.GetFileNameWithoutExtension(path);

                // Compare the entire line text with the filename
                if (lineText.Equals(filename, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    var content = File.ReadAllText(path);

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