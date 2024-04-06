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
                Selection.SelectLine();
                var textTrim = Selection.Text.Trim();
                if (textTrim.Equals("SEL", StringComparison.OrdinalIgnoreCase))
                {
                    var query = "SELECT TOP 100 * FROM [TableName] WHERE";
                    var editPoint = Selection.ActivePoint.CreateEditPoint();
                    editPoint.Insert(query);

                    Selection.MoveToPoint(editPoint);


                    CancelKeypress = true;
                }
                else
                {
                    Selection.EndOfLine();
                }
            }
        }
    }
}