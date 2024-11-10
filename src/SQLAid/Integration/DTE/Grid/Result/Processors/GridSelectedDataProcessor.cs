using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Integration.DTE.Grid.Result;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SQLAid.Integration.DTE.Grid.Processors
{
    public class GridSelectedDataProcessor : GridDataProcessorBase
    {
        public GridSelectedDataProcessor(
            IGridControl gridControl,
            IColumnTypeConverter typeConverter,
            IGridDataFormatter gridDataFormatter) : base(gridControl, typeConverter, gridDataFormatter)
        {
        }

        public override DataTable GetSchema()
        {
            return CreateSelectedColumnsSchema();
        }

        public override DataTable GridAsDatatable()
        {
            var schema = CreateSelectedColumnsSchema();
            PopulateSelectedData(schema);
            return schema;
        }

        private DataTable CreateSelectedColumnsSchema()
        {
            var schema = new DataTable();
            var columnTracker = new ColumnNameTracker();
            foreach (var cell in GridControl.SelectedCells.Cast<BlockOfCells>())
            {
                for (var col = cell.X; col <= cell.Right; col++)
                {
                    var (type, name) = _metadata.GetColumnInfo(col);
                    var columnName = columnTracker.GetUniqueColumnName(name);
                    schema.Columns.Add(columnName, type);
                }
            }

            return schema;
        }

        private void PopulateSelectedData(DataTable schema)
        {
            var selectedCells = GridControl.SelectedCells.Cast<BlockOfCells>();
            foreach (var cell in selectedCells)
            {
                ProcessSelectedCell(cell, schema);
            }

            schema.AcceptChanges();
        }

        private void ProcessSelectedCell(BlockOfCells cell, DataTable schema)
        {
            for (var rowIndex = cell.Y; rowIndex <= cell.Bottom; rowIndex++)
            {
                var rowData = GetRowData(cell, rowIndex, schema.Columns);
                schema.Rows.Add(rowData);
            }
        }

        private object[] GetRowData(BlockOfCells cell, long rowIndex, DataColumnCollection columns)
        {
            var rowData = new object[columns.Count];
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var gridColumnIndex = cell.X + columnIndex;
                if (gridColumnIndex <= cell.Right)
                {
                    var cellText = GridControl.GridStorage.GetCellDataAsString(rowIndex, gridColumnIndex);
                    rowData[columnIndex] = TypeConverter.ConvertToType(cellText, columns[columnIndex].DataType);
                }
            }
            return rowData;
        }

        public override IEnumerable<IEnumerable<string>> ConvertToSqlValues()
        {
            var selectedCells = GridControl.SelectedCells.Cast<BlockOfCells>();
            var result = new List<IEnumerable<string>>();
            foreach (var cell in selectedCells)
            {
                var cellValues = ProcessSelectedCell(cell);
                result.add
            }
        }
    }
}