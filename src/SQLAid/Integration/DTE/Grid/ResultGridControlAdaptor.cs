using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SQLAid.Integration.DTE.Grid
{
    /// <summary>
    /// Defines methods for converting grid cell values to typed and SQL formats
    /// </summary>
    public interface IGridCellValueConverter
    {
        object ConvertToTypedValue(string cellText, Type targetType);

        object ConvertToSqlValue(string cellText, Type columnType);
    }

    /// <summary>
    /// Default implementation for converting grid cell values
    /// </summary>
    public class DefaultGridCellValueConverter : IGridCellValueConverter
    {
        private static Dictionary<Type, Func<string, object>> TypeConverters = new Dictionary<Type, Func<string, object>>
        {
            [typeof(bool)] = text => text != "0",
            [typeof(Guid)] = text => new Guid(text),
            [typeof(DateTime)] = text => DateTime.Parse(text),
            [typeof(DateTimeOffset)] = text => DateTime.Parse(text),
            [typeof(byte[])] = text => Encoding.UTF8.GetBytes(text)
        };

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(decimal),
            typeof(long),
            typeof(double),
            typeof(float),
            typeof(byte)
        };

        private static readonly HashSet<Type> QuotedTypes = new HashSet<Type>
        {
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(byte[])
        };

        public object ConvertToTypedValue(string cellText, Type targetType)
        {
            if (string.IsNullOrEmpty(cellText) || cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return null;

            return TypeConverters.TryGetValue(targetType, out var converter)
                ? converter(cellText)
                : Convert.ChangeType(cellText, targetType, CultureInfo.InvariantCulture);
        }

        public object ConvertToSqlValue(string cellText, Type columnType)
        {
            if (string.IsNullOrEmpty(cellText) || cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return cellText;

            if (columnType == typeof(bool))
                return cellText == "1" ? 1 : 0;

            if (NumericTypes.Contains(columnType))
                return Convert.ChangeType(cellText, columnType, CultureInfo.InvariantCulture);

            var stringValue = Convert.ChangeType(cellText, typeof(string), CultureInfo.InvariantCulture);
            return QuotedTypes.Contains(columnType)
                ? $"N'{stringValue}'"
                : $"N'{EscapeSqlString(stringValue.ToString())}'";
        }

        private static string EscapeSqlString(string input) => input.Replace("'", "''");
    }

    /// <summary>
    /// Represents column metadata
    /// </summary>
    public class GridColumnInfo
    {
        public string Name { get; }
        public Type DataType { get; }

        public GridColumnInfo(string name, Type dataType)
        {
            Name = name;
            DataType = dataType;
        }
    }

    /// <summary>
    /// Provides data access and conversion for grid control
    /// </summary>
    public class GridDataProvider
    {
        private readonly IGridControl _gridControl;
        private readonly IGridCellValueConverter _valueConverter;
        private readonly DataTable _schemaTable;

        public GridDataProvider(IGridControl gridControl, IGridCellValueConverter valueConverter)
        {
            _gridControl = gridControl ?? throw new ArgumentNullException(nameof(gridControl));
            _valueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
            _schemaTable = _gridControl.GridStorage.GetField<DataTable>("m_schemaTable");
        }

        public string GetCellText(long rowIndex, int columnIndex) =>
            _gridControl.GridStorage.GetCellDataAsString(rowIndex, columnIndex) ?? string.Empty;

        public object GetTypedCellValue(long rowIndex, int columnIndex) =>
            _valueConverter.ConvertToTypedValue(GetCellText(rowIndex, columnIndex), GetColumnType(columnIndex));

        public object GetSqlCellValue(long rowIndex, int columnIndex) =>
            _valueConverter.ConvertToSqlValue(GetCellText(rowIndex, columnIndex), GetColumnType(columnIndex));

        public GridColumnInfo GetColumnInfo(int columnIndex)
        {
            if (columnIndex <= 0 || columnIndex >= _schemaTable.Rows.Count + 1)
                return new GridColumnInfo(string.Empty, typeof(string));

            var columnType = _schemaTable.Rows[columnIndex - 1][12].As<Type>();
            _gridControl.GetHeaderInfo(columnIndex, out var columnName, out Bitmap _);
            return new GridColumnInfo(columnName, columnType);
        }

        public Type GetColumnType(int columnIndex) =>
            (columnIndex <= 0 || columnIndex >= _schemaTable.Rows.Count + 1)
                ? typeof(string)
                : _schemaTable.Rows[columnIndex - 1][12].As<Type>();

        public string CleanColumnName(string rawName) =>
            !rawName.StartsWith("<") ? rawName : ExtractColumnName(rawName);

        private static string ExtractColumnName(string rawName)
        {
            try
            {
                var parts = rawName.Split('(');
                return parts.Length > 1 ? parts[1].TrimEnd(")>".ToCharArray()) : rawName;
            }
            catch
            {
                return rawName;
            }
        }
    }

    /// <summary>
    /// Adapts grid control for data operations
    /// </summary>
    public class ResultGridControlAdaptor : IDisposable
    {
        private readonly IGridControl _gridControl;
        private readonly GridDataProvider _dataProvider;
        private bool _isDisposed;

        public long RowCount => _gridControl?.GridStorage?.NumRows() ?? 0;
        public long ColumnCount => _gridControl.ColumnsNumber;

        public ResultGridControlAdaptor(IGridControl gridControl)
        {
            _gridControl = gridControl ?? throw new ArgumentNullException(nameof(gridControl));
            _dataProvider = new GridDataProvider(_gridControl, new DefaultGridCellValueConverter());
        }

        public DataTable GetSchemaTable() =>
            CreateDataTableFromColumns(1, ColumnCount, col => _dataProvider.GetColumnInfo(col));

        public DataTable GetDataTable()
        {
            var dataTable = GetSchemaTable();
            PopulateDataTable(dataTable);
            return dataTable;
        }

        public string GetColumnName(int columnIndex) => _dataProvider.GetColumnInfo(columnIndex).Name;

        public Dictionary<string, List<object>> GetSelectedCellsAsDictionary()
        {
            var result = new Dictionary<string, List<object>>();
            var gridHeader = _gridControl.GetField<GridHeader>("m_gridHeader");

            foreach (BlockOfCells cell in _gridControl.SelectedCells)
            {
                ProcessSelectedBlock(cell, result, gridHeader);
            }

            return result;
        }

        public IEnumerable<string> GetSelectedCellsAsSqlQuery()
        {
            var selectedData = GetSelectedCellsAsDictionary();
            var columnNames = selectedData.Keys.Select(k => k.StartsWith("[") ? k : $"[{k}]");
            var columnJoins = string.Join(", ", columnNames);
            var rowData = selectedData.Values.ZipIt(values => string.Join(", ", values));

            return new[] { columnJoins }.Concat(rowData);
        }

        public DataTable GetSelectedCellsAsDataTable()
        {
            var dataTable = new DataTable();
            var selectedCells = _gridControl.SelectedCells.Cast<BlockOfCells>().ToList();

            if (selectedCells.Count == 0)
                return dataTable;

            AddColumnsFromSelection(dataTable, selectedCells);
            AddRowsFromSelection(dataTable, selectedCells);
            return dataTable;
        }

        public void SetColumnBackgroundRange(int startColumn, int endColumn, Color backgroundColor)
        {
            var columns = _gridControl.GetBaseTypeField<GridColumnCollection>("m_columns");
            for (int i = startColumn; i < endColumn && i < columns.Count; i++)
            {
                if (i >= 0)
                    columns[i].BackgroundBrush.Color = backgroundColor;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }

        private DataTable CreateDataTableFromColumns(int start, long end, Func<int, GridColumnInfo> columnInfoProvider)
        {
            var dataTable = new DataTable();
            for (int column = start; column < end; column++)
            {
                var info = columnInfoProvider(column);
                dataTable.Columns.Add(info.Name, info.DataType);
            }
            return dataTable;
        }

        private void PopulateDataTable(DataTable dataTable)
        {
            for (long row = 0; row < RowCount; row++)
            {
                var values = new object[dataTable.Columns.Count];
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    values[col] = _dataProvider.GetTypedCellValue(row, col + 1);
                }
                dataTable.Rows.Add(values);
            }
            dataTable.AcceptChanges();
        }

        private void ProcessSelectedBlock(BlockOfCells cell, Dictionary<string, List<object>> result, GridHeader gridHeader)
        {
            for (var row = cell.Y; row <= cell.Bottom; row++)
            {
                for (var col = cell.X; col <= cell.Right; col++)
                {
                    var columnName = _dataProvider.CleanColumnName(gridHeader[col].Text.Trim());
                    result.Add(columnName, new List<object>());
                    result[columnName].Add(_dataProvider.GetSqlCellValue(row, col));
                }
            }
        }

        private void AddColumnsFromSelection(DataTable dataTable, List<BlockOfCells> selectedCells)
        {
            foreach (var cell in selectedCells)
            {
                for (var col = cell.X; col <= cell.Right; col++)
                {
                    var info = _dataProvider.GetColumnInfo(col);
                    dataTable.Columns.Add(info.Name, info.DataType);
                }
            }
        }

        private void AddRowsFromSelection(DataTable dataTable, List<BlockOfCells> selectedCells)
        {
            foreach (var cell in selectedCells)
            {
                for (var row = cell.Y; row <= cell.Bottom; row++)
                {
                    var values = new List<object>();
                    for (var col = cell.X; col <= cell.Right; col++)
                    {
                        values.Add(GetTypedValueForColumn(dataTable, values.Count, row, col));
                    }
                    dataTable.Rows.Add(values.ToArray());
                }
            }
            dataTable.AcceptChanges();
        }

        private object GetTypedValueForColumn(DataTable dataTable, int columnIndex, long row, int column)
        {
            var value = _dataProvider.GetTypedCellValue(row, column);
            return dataTable.Columns[columnIndex].DataType == typeof(bool) && value != null
                ? (bool)value
                : value;
        }
    }
}