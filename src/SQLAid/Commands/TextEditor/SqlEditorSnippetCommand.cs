using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using SQLAid.Templates;
using System;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    /// <summary>
    /// Handles SQL snippet insertion in the editor based on text triggers
    /// </summary>
    internal sealed class SqlEditorSnippetCommand : IDisposable
    {
        private readonly ITemplateProvider _templateProvider;
        private readonly IEditorService _editorService;
        private readonly TextDocumentKeyPressEvents _keyPressEvents;
        private bool _isDisposed;

        public SqlEditorSnippetCommand(
            ITemplateProvider templateProvider,
            IEditorService editorService,
            Events2 events)
        {
            _templateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
            _editorService = editorService ?? throw new ArgumentNullException(nameof(editorService));
            _keyPressEvents = events?.TextDocumentKeyPressEvents ?? throw new ArgumentNullException(nameof(events));

            _keyPressEvents.BeforeKeyPress += HandleBeforeKeyPress;
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var events = sqlAsyncPackage.Application.Events as Events2;
            if (events == null)
                return;

            var command = new SqlEditorSnippetCommand(
                new TemplateProvider(
                    sqlAsyncPackage.ExtensionInstallationDirectory,
                    sqlAsyncPackage.Options.TemplateDirectory),
                new EditorService(), events);
        }

        private void HandleBeforeKeyPress(string keypress, TextSelection selection, bool inStatementCompletion, ref bool cancelKeypress)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (keypress != "\t")
                return;

            var lineInfo = _editorService.GetCurrentLineInfo(selection);
            if (lineInfo == null)
                return;

            cancelKeypress = TryInsertSnippet(selection, lineInfo);
        }

        private bool TryInsertSnippet(TextSelection selection, LineInfo lineInfo)
        {
            if (string.IsNullOrWhiteSpace(lineInfo.Text))
                return false;

            var template = _templateProvider.FindTemplate(lineInfo.Text);
            if (string.IsNullOrWhiteSpace(template))
                return false;

            _editorService.ReplaceLineWithTemplate(selection, lineInfo, template);
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _keyPressEvents.BeforeKeyPress -= HandleBeforeKeyPress;
            _isDisposed = true;
        }
    }
}