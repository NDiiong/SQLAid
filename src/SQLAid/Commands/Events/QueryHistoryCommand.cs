﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration.DTE;
using SQLAid.Logging;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.Events
{
    // ADD GUI FOR HISTORY
    internal sealed class QueryHistoryCommand
    {
        private static CommandEvents _executeEvent;
        private static SqlAsyncPackage _sqlAsyncPackage;
        private static readonly ConcurrentQueue<QueryItem> _itemsQueue = new ConcurrentQueue<QueryItem>();

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _sqlAsyncPackage = package;
            var commandService = _sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var command = _sqlAsyncPackage.Application.Commands.Item("Query.Execute");
            _executeEvent = _sqlAsyncPackage.Application.Events.get_CommandEvents(command.Guid, command.ID);
            _executeEvent.BeforeExecute += (string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault) => SaveQueryEvent(_sqlAsyncPackage.Application);
            _executeEvent.AfterExecute += CommandEvents_AfterExecute;
        }

        private static void SaveQueryEvent(DTE2 dte)
        {
            var queryText = GetQueryText(dte);
            if (string.IsNullOrWhiteSpace(queryText))
                return;

            _itemsQueue.Enqueue(new QueryItem
            {
                Query = queryText,
                ExecutionDate = DateTime.Now,
            });

            Task.Delay(1000).ContinueWith((t) => SaveHistories());
        }

        private static void CommandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            Logger.Info("CommandEvents_AfterExecute");
        }

        private static void SaveHistories()
        {
            lock (_itemsQueue)
            {
                while (_itemsQueue.TryDequeue(out var item))
                {
                    var path = Path.Combine(_sqlAsyncPackage.Options.HistoryDirectory, item.ExecutionDate.ToString("dd.MM.yyyy.hh.mm.ss.fff") + Path.GetRandomFileName());
                    File.AppendAllText(path + ".txt", $"{item.ExecutionDate}" + Environment.NewLine + item.Query);
                }
            }
        }

        private static string GetQueryText(_DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var document = dte.ActiveDocument;
            if (document == null)
                return null;

            var textDocument = (TextDocument)document.Object("TextDocument");
            var queryText = textDocument.Selection.Text;

            if (string.IsNullOrEmpty(queryText))
            {
                var startPoint = textDocument.StartPoint.CreateEditPoint();
                queryText = startPoint.GetText(textDocument.EndPoint);
            }

            return queryText;
        }

        private class QueryItem
        {
            public string Query { get; set; }
            public DateTime ExecutionDate { get; set; }
        }
    }
}