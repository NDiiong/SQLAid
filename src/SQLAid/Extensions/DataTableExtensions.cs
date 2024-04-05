#pragma warning disable IDE1006

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SQLAid.Extensions
{
    public static class DataTableExtensions
    {
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
    }
}