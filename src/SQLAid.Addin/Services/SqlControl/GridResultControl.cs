using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Services.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace SQLAid.Services.SqlControl
{
    public class GridResultControl : IDisposable
    {
        private readonly GridControl _gridControl;

        public int ColumnCount { get; private set; }

        public long RowCount { get; private set; }

        public bool IsDisposed { get; private set; }

        public GridResultControl(GridControl gridControl)
        {
            _gridControl = gridControl ?? throw new ArgumentNullException(nameof(gridControl));
            ColumnCount = gridControl.ColumnsNumber;
            RowCount = gridControl.GridStorage?.NumRows() ?? throw new ArgumentNullException(nameof(gridControl.GridStorage));
        }

        public string GetCellValue(long nRowIndex, int nColIndex)
        {
            return _gridControl.GridStorage.GetCellDataAsString(nRowIndex, nColIndex);
        }

        public string[] GetColumnHeaders()
        {
            var columnHeaders = new string[ColumnCount - 1];
            for (var colIndex = 1; colIndex < ColumnCount; colIndex++)
            {
                columnHeaders[colIndex - 1] = GetColumnHeader(colIndex);
            }

            return columnHeaders;
        }

        public DataTable GetSchemaTableColumnHeaders()
        {
            return _gridControl.GridStorage.GetNonPublicField("m_schemaTable").As<DataTable>();
        }

        public IEnumerable<(Type, string)> GetDetailColumnHeaders()
        {
            var result = new List<(Type, string)>();
            var schema = _gridControl.GridStorage.GetNonPublicField("m_schemaTable").As<DataTable>();
            for (var colIndex = 1; colIndex < ColumnCount; colIndex++)
            {
                result.Add((schema.Rows[colIndex - 1][12].As<Type>(), GetColumnHeader(colIndex)));
            }

            return result;
        }

        public string GetColumnHeader(int nColIndex)
        {
            if (nColIndex > 0 && nColIndex < ColumnCount)
            {
                _gridControl.GetHeaderInfo(nColIndex, out string result, out Bitmap _);
                return result;
            }

            return string.Empty;
        }

        public string[] GetStringColumnHeadersInserted()
        {
            var columnHeaders = new string[ColumnCount - 1];
            for (var colIndex = 1; colIndex < ColumnCount; colIndex++)
            {
                var columnName = GetColumnHeader(colIndex);
                if (columnHeaders.Contains("[" + columnName + "]"))
                {
                    columnName = columnName + "_" + colIndex.ToString();
                }

                columnHeaders[colIndex - 1] = "[" + columnName + "]";
            }

            return columnHeaders;
        }

        public DataTable AsDatatable()
        {
            var datatable = new DataTable();
            var headers = GetDetailColumnHeaders();
            foreach (var (type, name) in headers)
            {
                datatable.Columns.Add(name, type);
            }

            for (var nRowIndex = 0L; nRowIndex < RowCount; nRowIndex += 1L)
            {
                var row = datatable.NewRow();
                for (var nColIndex = 1; nColIndex < ColumnCount; nColIndex++)
                {
                    var cellText = GetCellValue(nRowIndex, nColIndex) ?? "";

                    if (!cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        var column = datatable.Columns[nColIndex - 1];
                        if (column.DataType == typeof(bool))
                            cellText = cellText == "0" ? "False" : "True";

                        var typedValue = Convert.ChangeType(cellText, column.DataType, CultureInfo.InvariantCulture);
                        row[nColIndex - 1] = typedValue;
                    }
                }

                datatable.Rows.Add(row);
            }

            return datatable;
        }

        public IEnumerable<IEnumerable<string>> GetStringRowsInserted()
        {
            var contentRows = new List<List<string>>();
            for (var nRowIndex = 0L; nRowIndex < RowCount; nRowIndex += 1L)
            {
                var row = new List<string>();
                for (var nColIndex = 1; nColIndex < ColumnCount; nColIndex++)
                {
                    var cellText = GetCellValue(nRowIndex, nColIndex) ?? "";
                    cellText = cellText.Replace("'", "''");

                    if (!cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        cellText = "N'" + cellText + "'";
                    }

                    row.Add(cellText);
                }

                contentRows.Add(row);
            }

            return contentRows;
        }

        public long GetSelectedRow()
        {
            return GetSelectedCell()?.RowIndex ?? -1;
        }

        public int GetSelectedColumn()
        {
            return GetSelectedCell()?.ColumnIndex + 1 ?? -1;
        }

        public GridResultSelectedCell GetSelectedCell()
        {
            var selectedCells = GetSelectedCells();

            if (selectedCells.LongCount() > 0)
                return selectedCells.ElementAt(0);

            return default;
        }

        public IEnumerable<GridResultSelectedCell> GetSelectedCells()
        {
            return _gridControl.SelectedCells
                .Cast<BlockOfCells>()
                .Select((Func<BlockOfCells, GridResultSelectedCell>)((item) => item));
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            GC.ReRegisterForFinalize(ColumnCount);
            GC.ReRegisterForFinalize(RowCount);
            GC.ReRegisterForFinalize(_gridControl);
            GC.ReRegisterForFinalize(this);

            IsDisposed = true;
        }
    }
}