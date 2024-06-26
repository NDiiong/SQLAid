﻿using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlPasteAsCsvCommand
    {
        private const string PATTERN = @"[ \t]*\r?\n[ \t]*";
        private static readonly IClipboardService _clipboardService;
        private static readonly IFrameDocumentView _frameDocumentView;

        static SqlPasteAsCsvCommand()
        {
            _frameDocumentView = new FrameDocumentView();
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsCsvCommand);
            var menuItem = new OleMenuCommand((s, e) => Execute(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => CanExecute(s);
            commandService.AddCommand(menuItem);
        }

        private static void CanExecute(object s)
        {
            try
            {
                var oleMenuCommand = s as OleMenuCommand;

                if (!string.IsNullOrWhiteSpace(_clipboardService.GetFromClipboard()))
                    oleMenuCommand.Visible = true;
            }
            catch (Exception)
            {
            }
        }

        private static void Execute()
        {
            var content = _clipboardService.GetFromClipboard();
            if (!string.IsNullOrWhiteSpace(content))
            {
                var textSelection = _frameDocumentView.GetTextSelection();
                var currentline = textSelection.TopPoint.Line;
                var currentColumn = textSelection.TopPoint.DisplayColumn;

                var replacement = "'," + Environment.NewLine + "'";
                var singleLine = $"'{Regex.Replace(content, PATTERN, replacement, RegexOptions.Multiline)}'";

                var ed = textSelection.TopPoint.CreateEditPoint();
                ed.Insert(singleLine);
                textSelection.MoveToLineAndOffset(currentline, currentColumn);
            }
        }
    }
}