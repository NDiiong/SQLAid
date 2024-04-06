using SQLAid.Extensions;
using SQLAid.Integration;
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SQLAid.Windows
{
    public partial class SeedingDataForm : Form
    {
        private readonly string _template;
        private readonly DataTable _dataTable;
        private readonly IClipboardService _clipboardService;

        public SeedingDataForm(IClipboardService clipboardService, string template, DataTable dataTable) : base()
        {
            InitializeComponent();
            _template = template;
            _dataTable = dataTable;
            _clipboardService = clipboardService;
        }

        private void AddOnClick(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTableName.Text))
                return;


            if (_dataTable.Columns != null && _dataTable.Columns.Count > 0)
            {
                var firstColumn = _dataTable.Columns[0];
                if (firstColumn.ColumnName.Contains("id", StringComparison.OrdinalIgnoreCase))
                {
                    var scriptseedData = new StringBuilder();

                    foreach (DataRow row in _dataTable.Rows)
                    {
                        var values = new StringBuilder();
                        var columns = new StringBuilder();
                        var seedData = new StringBuilder();

                        var firstColumnName = "";
                        var firstValueName = "";
                        values.Append("(");
                        seedData.Append(_template);

                        for (int i = 0; i < _dataTable.Columns.Count; i++)
                        {
                            if (i > 0)
                            {
                                values.Append(", ");
                                columns.Append(", ");
                            }

                            if (i == 0)
                            {
                                firstColumnName = _dataTable.Columns[i].ColumnName;
                                firstValueName = row[i].ToString();
                            }

                            columns.Append(_dataTable.Columns[i].ColumnName);
                            Type dataType = _dataTable.Columns[i].DataType;

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

                        seedData = seedData.Replace("{tableName}", $"{txtTableName.Text}");
                        seedData = seedData.Replace("{key}", $"{firstColumnName} = {firstValueName}");
                        seedData = seedData.Replace("{columnHeaders}", columns.ToString());
                        seedData = seedData.Replace("{rows}", values.ToString());

                        scriptseedData.AppendLine(seedData.ToString());
                    }

                    _clipboardService.Set(scriptseedData.ToString());

                    this.Close();
                }
            }
        }
    }
}