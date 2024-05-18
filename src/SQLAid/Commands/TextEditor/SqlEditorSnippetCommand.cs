using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeihanLi.Extensions;
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

            var fileTemplates = Directory.GetFiles(_sqlAsyncPackage.Options.TemplateDirectory).ToList();
            fileTemplates.AddRangeIf(x => !Path.GetFileName(x).StartsWith("SQL"), Directory.GetFiles($"{_sqlAsyncPackage.ExtensionInstallationDirectory}/Templates"));
            CancelKeypress = CreateSnippet(Selection, startPoint, fileTemplates);
        }

        private static bool CreateSnippet(TextSelection Selection, EditPoint startPoint, List<string> paths)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (var path in paths)
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                var keyword = startPoint.GetText(filename.Length + 1).ToLower();
                if (keyword.Equals(filename, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    var content = File.ReadAllText(path);
                    startPoint.Delete(filename.Length);
                    startPoint.Insert(content);
                    Selection.MoveToPoint(startPoint);
                    return true;
                }
            }

            return false;
        }
    }
}