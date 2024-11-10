using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Integration.DTE.Grid.Result;
using System.Data;

namespace SQLAid.Integration.DTE.Grid.Processors
{
    public class GridFocusDataProcessor : GridDataProcessorBase
    {
        public GridFocusDataProcessor(
            IGridControl gridControl,
            IColumnTypeConverter typeConverter,
            IGridDataFormatter gridDataFormatter) : base(gridControl, typeConverter, gridDataFormatter)
        {
        }

        public override DataTable GetSchema()
        {
            var schema = new DataTable();
            var columnTracker = new ColumnNameTracker();
            for (var column = 1; column < GridControl.ColumnsNumber; column++)
            {
                var (type, name) = _metadata.GetColumnInfo(column);
                var columnName = columnTracker.GetUniqueColumnName(name);
                schema.Columns.Add(columnName, type);
            }

            return schema;
        }

        public override DataTable GridAsDatatable()
        {
            var schema = GetSchema();
            var dataPopulator = new DataTablePopulator(GridControl, TypeConverter);
            var rowCount = GridControl?.GridStorage?.NumRows() ?? 0;
            return dataPopulator.PopulateTable(schema, rowCount);
        }
    }

    public class DataTablePopulator
    {
        private readonly IGridControl _gridControl;
        private readonly IColumnTypeConverter _typeConverter;

        public DataTablePopulator(IGridControl gridControl, IColumnTypeConverter typeConverter)
        {
            _gridControl = gridControl;
            _typeConverter = typeConverter;
        }

        public DataTable PopulateTable(DataTable schema, long rowCount)
        {
            for (var rowIndex = 0L; rowIndex < rowCount; ++rowIndex)
            {
                var rowData = GetRowData(rowIndex, schema.Columns);
                schema.Rows.Add(rowData);
            }

            schema.AcceptChanges();
            return schema;
        }

        private object[] GetRowData(long rowIndex, DataColumnCollection columns)
        {
            var rowData = new object[columns.Count];
            for (var i = 0; i < columns.Count; i++)
            {
                var colIndex = i + 1;
                var cellText = _gridControl.GridStorage.GetCellDataAsString(rowIndex, colIndex);
                rowData[i] = _typeConverter.ConvertToType(cellText, columns[i].DataType);
            }

            return rowData;
        }
    }
}