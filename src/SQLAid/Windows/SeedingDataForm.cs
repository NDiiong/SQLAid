using SQLAid.Extensions;
using SQLAid.Integration;
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace SQLAid.Windows
{
    public partial class SeedingDataForm : Form
    {
        private readonly string _template;
        private readonly DataTable _dataTable;
        private readonly IClipboardService _clipboardService;
        private const string IdColumnSuffix = "id";

        public SeedingDataForm(IClipboardService clipboardService, string template, DataTable dataTable)
        {
            InitializeComponent();

            if (template == null)
                throw new ArgumentNullException(nameof(template));
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));
            if (clipboardService == null)
                throw new ArgumentNullException(nameof(clipboardService));

            _template = template;
            _dataTable = dataTable;
            _clipboardService = clipboardService;
        }

        private void AddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var seedingScript = GenerateSeedingScript();
                _clipboardService.Set(seedingScript);
                Close();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while generating the seeding script.", ex);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
                return false;

            if (_dataTable.Columns == null || _dataTable.Columns.Count == 0)
            {
                ShowInfo("No columns found in the table");
                return false;
            }

            if (!HasIdColumn())
            {
                ShowError("Could not find unique id column", null);
                return false;
            }

            return true;
        }

        private bool HasIdColumn()
        {
            return _dataTable.Columns[0].ColumnName.Contains(IdColumnSuffix, StringComparison.OrdinalIgnoreCase);
        }

        private string GenerateSeedingScript()
        {
            var content = new StringBuilder();
            foreach (DataRow row in _dataTable.Rows)
            {
                var seedData = GenerateSeedDataForRow(row);
                content.AppendLine(seedData);
            }
            return content.ToString();
        }

        private string GenerateSeedDataForRow(DataRow row)
        {
            var columns = GenerateColumnsList();
            var values = GenerateValuesList(row);
            var firstColumnInfo = GetFirstColumnInfo(row);

            return FormatSeedTemplate(columns, values, firstColumnInfo);
        }

        private string GenerateColumnsList()
        {
            var columns = new StringBuilder();
            for (int i = 0; i < _dataTable.Columns.Count; i++)
            {
                if (i > 0)
                    columns.Append(", ");
                columns.Append(_dataTable.Columns[i].ColumnName);
            }
            return columns.ToString();
        }

        private string GenerateValuesList(DataRow row)
        {
            var values = new StringBuilder("(");

            for (int i = 0; i < _dataTable.Columns.Count; i++)
            {
                if (i > 0)
                    values.Append(", ");

                values.Append(FormatValue(row, i));
            }

            values.Append(")");
            return values.ToString();
        }

        private string FormatValue(DataRow row, int columnIndex)
        {
            if (row.IsNull(columnIndex))
                return "NULL";

            var value = row[columnIndex];
            var dataType = _dataTable.Columns[columnIndex].DataType;

            if (dataType == typeof(bool))
                return ((bool)value ? 1 : 0).ToString();

            if (IsNumericType(dataType))
                return value.ToString();

            if (dataType == typeof(byte[]))
                return FormatByteArray((byte[])value);

            return FormatString(value.ToString());
        }

        private bool IsNumericType(Type type)
        {
            return type == typeof(int) ||
                   type == typeof(decimal) ||
                   type == typeof(long) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(byte);
        }

        private string FormatByteArray(byte[] bytes)
        {
            var result = new StringBuilder("0x");
            foreach (byte b in bytes)
            {
                result.Append(b.ToString("x2"));
            }
            return result.ToString();
        }

        private string FormatString(string value)
        {
            return string.Format("N'{0}'", value.Replace("'", "''"));
        }

        private FirstColumnInfo GetFirstColumnInfo(DataRow row)
        {
            return new FirstColumnInfo
            {
                ColumnName = _dataTable.Columns[0].ColumnName,
                Value = row[0].ToString()
            };
        }

        private string FormatSeedTemplate(string columns, string values, FirstColumnInfo firstColumnInfo)
        {
            var seedData = new StringBuilder(_template);
            seedData.Replace("{tableName}", txtTableName.Text);
            seedData.Replace("{key}", string.Format("{0} = {1}", firstColumnInfo.ColumnName, firstColumnInfo.Value));
            seedData.Replace("{columnHeaders}", columns);
            seedData.Replace("{rows}", values);

            return seedData.ToString();
        }

        private void ShowError(string message, Exception ex)
        {
            var fullMessage = ex != null
                ? string.Format("{0}\n\nDetails: {1}", message, ex.Message)
                : message;
            MessageBox.Show(fullMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private class FirstColumnInfo
        {
            public string ColumnName { get; set; }
            public string Value { get; set; }
        }
    }
}