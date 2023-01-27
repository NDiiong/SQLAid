using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Addin.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridControlAdaptor : IDisposable
    {
        public int ColumnCount { get; }
        public long RowCount { get; }
        public bool IsDisposed { get; private set; }

        private readonly IGridControl _gridControl;

        public ResultGridControlAdaptor(IGridControl gridControl)
        {
            _gridControl = gridControl ?? throw new ArgumentNullException(nameof(gridControl));
            ColumnCount = gridControl.ColumnsNumber;
            RowCount = gridControl.GridStorage?.NumRows() ?? throw new ArgumentNullException(nameof(gridControl.GridStorage));
        }

        public string GetCellValue(long nRowIndex, int nColIndex)
        {
            var cellText = _gridControl.GridStorage.GetCellDataAsString(nRowIndex, nColIndex) ?? "";
            cellText = cellText.Replace("'", "''");

            if (!cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                cellText = "N'" + cellText + "'";

            return cellText;
        }

        public IEnumerable<(Type, string)> GetColumnTypes()
        {
            var result = new List<(Type, string)>();
            var schema = _gridControl.GridStorage.GetField<DataTable>("m_schemaTable");
            for (var column = 1; column < ColumnCount; column++)
            {
                var columnType = schema.Rows[column - 1][12].As<Type>();
                var columnText = GetColumnName(column);
                result.Add((columnType, columnText));
            }

            return result;
        }

        public string GetColumnName(int nColIndex)
        {
            if (nColIndex > 0 && nColIndex < ColumnCount)
            {
                _gridControl.GetHeaderInfo(nColIndex, out var columnName, out Bitmap _);
                return columnName;
            }

            return string.Empty;
        }

        public (Type, string) GetColumnType(int nColIndex)
        {
            if (nColIndex > 0 && nColIndex < ColumnCount)
            {
                var schema = _gridControl.GridStorage.GetField<DataTable>("m_schemaTable");
                var columnType = schema.Rows[nColIndex - 1][12].As<Type>();
                var columnText = GetColumnName(nColIndex);
                return (columnType, columnText);
            }

            return (default, string.Empty);
        }

        public string[] GetBracketColumns()
        {
            var columnHeaders = new string[ColumnCount - 1];
            for (var colIndex = 1; colIndex < ColumnCount; colIndex++)
            {
                var columnName = GetColumnName(colIndex);
                if (columnHeaders.Contains("[" + columnName + "]"))
                    columnName = columnName + "_" + colIndex.ToString();

                columnHeaders[colIndex - 1] = "[" + columnName + "]";
            }

            return columnHeaders;
        }

        public DataTable GridAsDatatable()
        {
            var datatable = new DataTable();
            var columnHeaders = GetColumnTypes();

            foreach (var (type, name) in columnHeaders)
                datatable.Columns.Add(name, type);

            for (var nRowIndex = 0L; nRowIndex < RowCount; ++nRowIndex)
            {
                var rows = new List<object>();
                for (var nColIndex = 1; nColIndex < ColumnCount; nColIndex++)
                {
                    var cellText = GetCellValue(nRowIndex, nColIndex);
                    if (!cellText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                    {
                        var column = datatable.Columns[nColIndex - 1];
                        if (column.DataType == typeof(bool))
                            cellText = cellText == "0" ? "False" : "True";

                        rows[nColIndex - 1] = Convert.ChangeType(cellText, column.DataType, CultureInfo.InvariantCulture);
                    }
                }

                datatable.Rows.Add(rows.ToArray());
            }

            datatable.AcceptChanges();
            return datatable;
        }

        public IEnumerable<IEnumerable<string>> GridAsQuerySql()
        {
            var rows = new List<List<string>>();
            for (var nRowIndex = 0L; nRowIndex < RowCount; ++nRowIndex)
            {
                var columns = new List<string>();
                for (var nColIndex = 1; nColIndex < ColumnCount; nColIndex++)
                {
                    var cellText = GetCellValue(nRowIndex, nColIndex);
                    columns.Add(cellText);
                }

                rows.Add(columns);
            }

            return rows;
        }

        public IEnumerable<string> GridSelectedAsQuerySql()
        {
            var resultGridSelected = GetResultGridSelected();
            var columnJoins = string.Join(", ", resultGridSelected.Select(q => "[" + q.Key + "]"));
            var rowJoins = resultGridSelected.Select(q => q.Value).ZipIt(xs => string.Join(", ", xs));
            var linkedList = new LinkedList<string>(rowJoins);
            linkedList.AddFirst(columnJoins);
            return linkedList;
        }

        public Dictionary<string, List<string>> GetResultGridSelected()
        {
            //var selectionManager = _gridControl.GetField<SelectionManager>("m_selMgr");
            var headers = _gridControl.GetField<GridHeader>("m_gridHeader");
            var gridResult = new Dictionary<string, List<string>>();
            foreach (BlockOfCells cell in _gridControl.SelectedCells)
            {
                for (var row = cell.Y; row <= cell.Bottom; row++)
                {
                    for (var col = cell.X; col <= cell.Right; col++)
                    {
                        var column = headers[col].Text.Trim();
                        if (column.StartsWith("<"))
                        {
                            try
                            {
                                var cols = column.Split('(');
                                if (cols.Length > 1)
                                    column = cols[1].TrimEnd(")>".ToArray());
                            }
                            catch { }
                        }

                        if (!gridResult.ContainsKey(column))
                            gridResult.Add(column, new List<string>());

                        var cellText = GetCellValue(row, col);
                        gridResult[column].Add(cellText);
                    }
                }
            }

            return gridResult;
        }

        //public IEnumerable<string> GridSelectedAsQuerySql()
        //{
        //    var contentRows = new List<string>();
        //    foreach (var cell in GetSelectedCells())
        //    {
        //        var cellText = GetCellValue(cell.RowIndex, cell.ColumnIndex);
        //        contentRows.Add(cellText);
        //    }

        //    return contentRows;
        //}

        public IEnumerable<ResultGridSelectedCell> GetSelectedCells()
        {
            return _gridControl.SelectedCells
                .Cast<BlockOfCells>()
                .Select((Func<BlockOfCells, ResultGridSelectedCell>)((item) => item));
        }

        public void SetColumnBackground(int columnIndex, Color backgroundColor)
        {
            var columnsCollection = _gridControl.GetBaseTypeField<GridColumnCollection>("m_columns");
            columnsCollection[columnIndex].BackgroundBrush.Color = backgroundColor;
        }

        public void SetRangeColumnBackground(int beginColumn, int endColumn, Color backgroundColor)
        {
            var columnsCollection = _gridControl.GetBaseTypeField<GridColumnCollection>("m_columns");
            for (var i = beginColumn; i < endColumn; i++)
            {
                columnsCollection[i].BackgroundBrush.Color = backgroundColor;
            }
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