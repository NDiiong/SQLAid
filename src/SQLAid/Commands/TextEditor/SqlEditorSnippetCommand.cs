using DocumentFormat.OpenXml.Drawing;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using System;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorSnippetCommand
    {
        private static TextDocumentKeyPressEvents _textDocumentKeyPressEvents;

        static SqlEditorSnippetCommand()
        {
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();


            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            var events = dte.Events as Events2;
            _textDocumentKeyPressEvents = events.TextDocumentKeyPressEvents;
            _textDocumentKeyPressEvents.BeforeKeyPress += _textDocumentKeyPressEvents_BeforeKeyPress;

        }

        private static void _textDocumentKeyPressEvents_BeforeKeyPress(string Keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Keypress == "\t")
            {
                var textDocument = Selection.Parent as TextDocument;
                var startPoint = Selection.ActivePoint.CreateEditPoint();
                startPoint.StartOfLine();

                while (!startPoint.AtEndOfLine && startPoint.GetText(1) == "\t")
                {
                    startPoint.CharRight();
                }

                if (startPoint.GetText(4).Equals("SEL", StringComparison.OrdinalIgnoreCase))
                {
                    startPoint.Delete(3);
                    startPoint.Insert("SELECT TOP 100 * FROM [table_name] WHERE ");
                    Selection.MoveToPoint(startPoint);

                    // Di chuyển đến vị trí cuối cùng của từ "table_name"
                    Selection.FindPattern("[table_name]", (int)vsFindOptions.vsFindOptionsMatchCase | (int)vsFindOptions.vsFindOptionsBackwards);

                    Selection.CharLeft(false, 1);
                    Selection.CharRight(true, "[table_name]".Length);

                    CancelKeypress = true;
                }
            }
        }
    }
}