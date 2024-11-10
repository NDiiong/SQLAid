using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Integration.DTE;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlEditorAutoAddBracketCommand : IDisposable
    {
        private readonly TextDocumentKeyPressEvents _textDocumentKeyPressEvents;
        private readonly IReadOnlyDictionary<char, char> _bracketPairs;
        private bool _isDisposed;

        public SqlEditorAutoAddBracketCommand(Events2 events)
        {
            _textDocumentKeyPressEvents = events?.TextDocumentKeyPressEvents
                ?? throw new ArgumentNullException(nameof(events));

            _bracketPairs = new Dictionary<char, char>
            {
                { '[', ']' },
                { '(', ')' },
                { ']', '[' },
                { ')', '(' }
            };

            _textDocumentKeyPressEvents.BeforeKeyPress += HandleBeforeKeyPress;
        }

        /// <summary>
        /// Initializes the SQL Editor Auto Bracket Command
        /// </summary>
        /// <param name="sqlAsyncPackage">The SQL Async Package instance</param>
        /// <returns>A task representing the initialization operation</returns>
        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            if (sqlAsyncPackage == null)
                throw new ArgumentNullException(nameof(sqlAsyncPackage));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var events = sqlAsyncPackage.Application.Events as Events2;
            var _ = new SqlEditorAutoAddBracketCommand(events);
        }

        /// <summary>
        /// Handles the key press event before it's processed
        /// </summary>
        private void HandleBeforeKeyPress(string keypress, TextSelection selection, bool inStatementCompletion, ref bool cancelKeypress)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (string.IsNullOrEmpty(keypress) || keypress.Length != 1)
                    return;

                char pressedKey = keypress[0];
                if (!_bracketPairs.ContainsKey(pressedKey))
                    return;

                var selectedText = selection?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(selectedText) || selectedText.StartsWith(keypress))
                    return;

                InsertBracketedText(selection, selectedText, pressedKey);
                cancelKeypress = true;
            }
            catch (Exception ex)
            {
                // Log the exception - replace with your logging framework
                System.Diagnostics.Debug.WriteLine($"Error in HandleBeforeKeyPress: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Inserts the selected text wrapped in matching brackets
        /// </summary>
        private void InsertBracketedText(TextSelection selection, string selectedText, char openingBracket)
        {
            if (selection == null)
                throw new ArgumentNullException(nameof(selection));

            char closingBracket = _bracketPairs[openingBracket];
            string wrappedText = $"{openingBracket}{selectedText}{closingBracket}";
            selection.Insert(wrappedText);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (_textDocumentKeyPressEvents != null)
            {
                _textDocumentKeyPressEvents.BeforeKeyPress -= HandleBeforeKeyPress;
            }

            _isDisposed = true;
        }
    }
}