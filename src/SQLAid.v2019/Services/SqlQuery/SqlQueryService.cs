using System.Text.RegularExpressions;

namespace SQLAid.Services.SqlQuery
{
    public class SqlQueryService : ISqlQueryService
    {
        private const string _template = "SELECT *\r\nFROM\r\n(\r\n\tVALUES\r\n{{_datatable}}\r\n)_tables({{_columns}})";

        public string Sanitize(string content, string columns)
        {
            var result = Regex.Replace(content, @"\t", "', '", RegexOptions.Multiline);
            var queryInsert = Regex.Replace(result, "(.*[^\r\n])", "\t('$1'),", RegexOptions.Multiline);
            queryInsert = queryInsert.TrimEnd(',');

            return _template
                .Replace("{{_datatable}}", queryInsert)
                .Replace("{{_columns}}", columns)
                .Replace("'NULL'", "NULL");
        }
    }
}