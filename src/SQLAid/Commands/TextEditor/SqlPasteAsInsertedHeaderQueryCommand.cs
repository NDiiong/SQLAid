﻿using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlPasteAsInsertedHeaderQueryCommand
    {
        private static readonly IClipboardService _clipboardService;
        private static readonly IFrameDocumentView _frameDocumentView;
        private static string templates;

        static SqlPasteAsInsertedHeaderQueryCommand()
        {
            _clipboardService = new ClipboardService();
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            templates = File.ReadAllText($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/Internal/SQL.INSERT.INTO.sql");
            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsInsertedWithHeaderCommand);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var content = _clipboardService.GetFromClipboard();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0)
                    {
                        var columns = lines.ElementAtOrDefault(0)?.Split('\t');
                        if (columns != null)
                        {
                            var result = new List<string>();
                            foreach (var item in lines.Skip(1))
                            {
                                var cells = item.Split('\t');
                                var @string = "";
                                for (int i = 0; i < cells.Length; i++)
                                {
                                    if (i > 0)
                                        @string += ", ";

                                    if (cells[i] != "NULL")
                                        @string += string.Format("N'{0}'", cells[i].Replace("'", "''"));
                                    else
                                        @string += cells[i];
                                }

                                result.Add(@string);
                            }

                            var rows = string.Join($",{Environment.NewLine}\t", result.Select(r => $"({string.Join(", ", r)})"));
                            var columnsJoined = string.Join(", ", columns.Select(x => x.StartsWith("[") ? x : $"[{x}]"));
                            var sqlQuery = templates.Replace("{rows}", rows).Replace("{columnHeaders}", columnsJoined);

                            var textSelection = _frameDocumentView.GetTextSelection();
                            var currentline = textSelection.TopPoint.Line;
                            var currentColumn = textSelection.TopPoint.DisplayColumn;
                            var editPoint = textSelection.TopPoint.CreateEditPoint();
                            editPoint.Insert(sqlQuery);
                            textSelection.MoveToLineAndOffset(currentline, currentColumn);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}