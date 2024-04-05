using System.Data;

namespace SQLAid.Integration.Files
{
    public class HtmlService : IFileService, IHtmlService
    {
        public string ToHtml(DataTable datatable)
        {
            string html = "<table>";

            //add header row
            html += "<tr>";
            for (int i = 0; i < datatable.Columns.Count; i++)
                html += "<td>" + datatable.Columns[i].ColumnName + "</td>";
            html += "</tr>";

            //add rows
            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < datatable.Columns.Count; j++)
                    html += "<td>" + datatable.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";

            return html;
        }

        public void WriteFile(string path, DataTable datatable)
        {
            System.IO.File.WriteAllText(path, ToHtml(datatable));
        }
    }
}