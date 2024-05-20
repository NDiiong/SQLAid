using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor
{
    internal sealed class SqlPasteAsInsertCommand
    {
        private readonly IClipboardService _clipboardService;
        private readonly IEditorService _editorService;
        private readonly IFrameDocumentView _documentView;
        private readonly ITemplateProvider _templateProvider;
        private readonly SqlInsertQueryBuilder _queryBuilder;
        private readonly ClipboardDataParser _dataParser;

        public SqlPasteAsInsertCommand(
             IClipboardService clipboardService,
             IEditorService editorService,
             IFrameDocumentView documentView,
             ITemplateProvider templateProvider,
             SqlInsertQueryBuilder queryBuilder,
             ClipboardDataParser dataParser)
        {
            _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
            _editorService = editorService ?? throw new ArgumentNullException(nameof(clipboardService));
            _documentView = documentView ?? throw new ArgumentNullException(nameof(documentView));
            _templateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
            _queryBuilder = queryBuilder ?? throw new ArgumentNullException(nameof(queryBuilder));
            _dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = sqlAsyncPackage.GetService<IMenuCommandService, OleMenuCommandService>();
            var cmdId = new CommandID(PackageGuids.guidCommands, PackageIds.PasteAsInsertedWithHeaderCommand);

            // Create command instance with dependencies
            var command = CreateCommandInstance(sqlAsyncPackage);

            var menuItem = new OleMenuCommand(async (s, e) => await command.ExecuteAsync(), cmdId);
            menuItem.BeforeQueryStatus += (s, e) => command.UpdateCommandStatus(s as OleMenuCommand);

            commandService.AddCommand(menuItem);
        }

        private static SqlPasteAsInsertCommand CreateCommandInstance(SqlAsyncPackage package)
        {
            var clipboardService = new ClipboardService();
            var editorService = new EditorService();
            var documentView = new FrameDocumentView();
            var templateProvider = new TemplateProvider(package.ExtensionInstallationDirectory);
            var queryBuilder = new SqlInsertQueryBuilder();
            var dataParser = new ClipboardDataParser();

            return new SqlPasteAsInsertCommand(clipboardService, editorService, documentView, templateProvider, queryBuilder, dataParser);
        }

        public void UpdateCommandStatus(OleMenuCommand menuCommand)
        {
            if (menuCommand == null)
                return;

            try
            {
                var clipboardContent = _clipboardService.GetFromClipboard();
                menuCommand.Visible = !string.IsNullOrWhiteSpace(clipboardContent);
            }
            catch (Exception)
            {
                menuCommand.Visible = false;
            }
        }

        public async Task ExecuteAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var clipboardContent = _clipboardService.GetFromClipboard();
            if (string.IsNullOrWhiteSpace(clipboardContent))
                return;

            var (headers, rows) = _dataParser.Parse(clipboardContent);
            var template = await _templateProvider.GetTemplateAsync("SQL.INSERT.INTO.sql");
            var sqlQuery = _queryBuilder.BuildQuery(template, headers, rows);

            var textSelection = _documentView.GetTextSelection();
            _editorService.InsertText(textSelection, sqlQuery);
            _editorService.RestoreCursorPosition(textSelection, new EditorPosition(textSelection.TopPoint.Line, textSelection.TopPoint.DisplayColumn));
        }

        internal class SqlInsertQueryBuilder
        {
            public string BuildQuery(string templateContent, string[] headers, IEnumerable<string[]> rows)
            {
                var formattedColumns = FormatColumnHeaders(headers);
                var formattedRows = FormatRows(rows);
                return templateContent
                    .Replace("{columnHeaders}", formattedColumns)
                    .Replace("{rows}", formattedRows);
            }

            private string FormatColumnHeaders(string[] headers)
            {
                return string.Join(", ", headers.Select(x =>
                    x.StartsWith("[") ? x : $"[{x}]"));
            }

            private string FormatRows(IEnumerable<string[]> rows)
            {
                var formattedRows = rows.Select(cells =>
                    $"({string.Join(", ", cells.Select(FormatCell))})");

                return string.Join($",{Environment.NewLine}\t", formattedRows);
            }

            private string FormatCell(string cell)
            {
                return cell == "NULL"
                    ? cell
                    : $"N'{cell.Replace("'", "''")}'";
            }
        }
    }
}