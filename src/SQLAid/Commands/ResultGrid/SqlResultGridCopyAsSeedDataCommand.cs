#pragma warning disable IDE1006

using Microsoft.VisualStudio.Shell;
using SQLAid.Extensions;
using SQLAid.Integration;
using SQLAid.Integration.Clipboard;
using SQLAid.Integration.DTE;
using SQLAid.Integration.DTE.Grid;
using SQLAid.Windows;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.ResultGrid
{
    internal sealed class SqlResultGridCopyAsSeedDataCommand : SqlResultGridCommandBase
    {
        private static string templates;
        private static readonly IClipboardService _clipboardService;

        static SqlResultGridCopyAsSeedDataCommand()
        {
            _clipboardService = new ClipboardService();
        }

        public static async Task InitializeAsync(SqlAsyncPackage sqlAsyncPackage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            templates = File.ReadAllText($"{sqlAsyncPackage.ExtensionInstallationDirectory}/Templates/SQL.SEED.DATA.sql");
            GridCommandBar.AddButton("Copy As #Seed Data", $"{sqlAsyncPackage.ExtensionInstallationDirectory}/Resources/Assets/insert-table.ico", OnClick);
        }

        private static void OnClick()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var t = new TableName();
            t.ShowDialog();

            var focusGridControl = GridControl.GetFocusGridControl();
            using (var gridResultControl = new ResultGridControlAdaptor(focusGridControl))
            {
                var dataTable = gridResultControl.GridFocusAsDatatable();
                if (dataTable.Columns != null && dataTable.Columns.Count > 0)
                {
                    var firstColumn = dataTable.Columns[0];
                    if (firstColumn.ColumnName.Contains("id", StringComparison.OrdinalIgnoreCase))
                    {
                        var scriptseedData = new StringBuilder();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            var values = new StringBuilder();
                            var columns = new StringBuilder();
                            var seedData = new StringBuilder();

                            var firstColumnName = "";
                            var firstValueName = "";
                            values.Append("(");
                            seedData.Append(templates);
                            for (int i = 0; i < dataTable.Columns.Count; i++)
                            {
                                if (i > 0)
                                {
                                    values.Append(", ");
                                    columns.Append(", ");
                                }

                                if (i == 0)
                                {
                                    firstColumnName = dataTable.Columns[i].ColumnName;
                                    firstValueName = row[i].ToString();
                                }

                                columns.Append(dataTable.Columns[i].ColumnName);
                                Type dataType = dataTable.Columns[i].DataType;

                                if (row.IsNull(i))
                                {
                                    values.Append("NULL");
                                }
                                else if (dataType == typeof(bool))
                                {
                                    values.Append((bool)row[i] ? 1 : 0);
                                }
                                else if (dataType == typeof(int) || dataType == typeof(decimal) || dataType == typeof(long) || dataType == typeof(double) || dataType == typeof(float) || dataType == typeof(byte))
                                {
                                    values.Append(row[i].ToString());
                                }
                                else if (dataType == typeof(byte[]))
                                {
                                    values.Append("0x");
                                    foreach (byte b in (byte[])row[i])
                                    {
                                        values.Append(b.ToString("x2"));
                                    }
                                }
                                else
                                {
                                    values.AppendFormat("N'{0}'", row[i].ToString().Replace("'", "''"));
                                }
                            }

                            values.AppendFormat(")");

                            seedData = seedData.Replace("{key}", $"{firstColumnName} = {firstValueName}");
                            seedData = seedData.Replace("{columnHeaders}", columns.ToString());
                            seedData = seedData.Replace("{rows}", values.ToString());

                            scriptseedData.AppendLine(seedData.ToString());
                        }

                        _clipboardService.Set(scriptseedData.ToString());
                    }
                }
            }
        }
    }
}