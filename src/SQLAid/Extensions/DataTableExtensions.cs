#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SQLAid.Extensions
{
    public static class DataTableExtensions
    {
        public static string FormatDataTable(DataTable table)
        {
            var builder = new StringBuilder();

            // Determine the maximum width of each column
            var columnWidths = new int[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columnWidths[i] = table.Columns[i].ColumnName.Length;
                foreach (DataRow row in table.Rows)
                {
                    string cellValue = row[i] == DBNull.Value ? "NULL" : row[i].ToString();
                    int length = cellValue.Length;
                    if (length > columnWidths[i])
                    {
                        columnWidths[i] = length;
                    }
                }
            }

            // Build the header line with column names
            builder.AppendLine(BuildSeparatorLine(columnWidths));
            builder.AppendLine(BuildRowLine(table.Columns, columnWidths));

            // Build the separator line after header
            builder.AppendLine(BuildSeparatorLine(columnWidths));

            // Build each row in the table
            foreach (DataRow row in table.Rows)
            {
                builder.AppendLine(BuildRowLine(row, columnWidths));
            }

            // Build the final separator line
            builder.AppendLine(BuildSeparatorLine(columnWidths));

            return builder.ToString();
        }

        private static string BuildSeparatorLine(int[] columnWidths)
        {
            var separator = new StringBuilder("+");
            foreach (int width in columnWidths)
            {
                separator.Append(new string('-', width + 2)).Append("+");
            }
            return separator.ToString();
        }

        private static string BuildRowLine(DataColumnCollection columns, int[] columnWidths)
        {
            var row = new StringBuilder("|");
            for (int i = 0; i < columns.Count; i++)
            {
                row.Append(" ").Append(columns[i].ColumnName.PadRight(columnWidths[i])).Append(" |");
            }
            return row.ToString();
        }

        private static string BuildRowLine(DataRow row, int[] columnWidths)
        {
            var rowLine = new StringBuilder("|");
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                // Handle DBNull by replacing with "NULL" or truncate to max length
                string cellValue = row[i] == DBNull.Value ? "NULL" : row[i].ToString();
                cellValue = cellValue.Length > columnWidths[i] ? cellValue.Substring(0, columnWidths[i]) : cellValue;
                rowLine.Append(" ").Append(cellValue.PadRight(columnWidths[i])).Append(" |");
            }
            return rowLine.ToString();
        }

        public static string ToCsv(this DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns
                .Cast<DataColumn>()
                .Select(column => column.ColumnName);

            sb.AppendLine(string.Join(",", columnNames));
            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
                  string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString();
        }

        public static string ToTeamsFormat(this DataTable dt)
        {
            string html = "<table>";
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
    }
}