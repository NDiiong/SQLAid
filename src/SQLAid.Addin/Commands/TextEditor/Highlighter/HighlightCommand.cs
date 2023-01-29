using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using SQLAid.Addin.Logging;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor.Highlighter
{
    public static class HighlightCommand
    {
        private static WindowEvents _windowEvents;
        private static IServiceProvider _serviceProvider;
        private static IWpfTextView _textView;
        private static IFrameDocumentView _frameDocumentView;

        static HighlightCommand()
        {
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _serviceProvider = package;
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            _windowEvents = dte.Events.WindowEvents;
            _windowEvents.WindowCreated += _windowEvents_WindowCreated;
        }

        private static void _windowEvents_WindowCreated(Window Window)
        {
            try
            {
                var vsTextManager = _serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
                var componentModel = (IComponentModel)_serviceProvider.GetService(typeof(SComponentModel));
                var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();

                IVsTextView textViewCurrent;
                vsTextManager.GetActiveView(1, null, out textViewCurrent);
                _textView = editor.GetWpfTextView(textViewCurrent);

                _textView.Selection.SelectionChanged += Selection_SelectionChanged;

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                throw;
            }
        }

        private static void Selection_SelectionChanged(object sender, EventArgs e)
        {
            var wpfTextViewLineCollection = _textView.TextViewLines;
            var textDocument = _frameDocumentView.GetTextDocument();
            var text = textDocument.CreateEditPoint().GetText(textDocument.EndPoint);
            UpdateMatches();
            Console.WriteLine();
        }
        private static bool UpdateMatches(bool wereAdornmentsEnabled = true)
        {
            var _highlightSpan = SelectionToHighlightSpan();

            var text = _highlightSpan.HasValue ? _highlightSpan.Value.GetText() : null;
            Logger.Info(text);

            return true;
        }

        private static SnapshotSpan? SelectionToHighlightSpan()
        {
            var selection = _textView.Selection.StreamSelectionSpan.SnapshotSpan;

            if ((selection.Length > 0) && (selection.Length < 128))
            {
                if (_textView.Selection.Mode == TextSelectionMode.Box)
                {
                    ITextViewLine line = _textView.GetTextViewLineContainingBufferPosition(_textView.Selection.ActivePoint.Position);
                    if (!line.ContainsBufferPosition(_textView.Selection.AnchorPoint.Position))
                        return null;
                }

                int end = selection.End;
                for (int i = selection.Start; (i < end); ++i)
                {
                    var c = selection.Snapshot[i];
                    if (!char.IsWhiteSpace(c))
                        return selection;
                }
            }

            return null;
        }
    }
}