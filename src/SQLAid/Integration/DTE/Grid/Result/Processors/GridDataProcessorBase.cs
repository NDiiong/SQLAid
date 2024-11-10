using Microsoft.SqlServer.Management.UI.Grid;
using SQLAid.Integration.DTE.Grid.Result;
using System.Collections.Generic;
using System.Data;

namespace SQLAid.Integration.DTE.Grid.Processors
{
    public interface IGridDataProcessor
    {
        DataTable GetSchema();

        DataTable GridAsDatatable();

        IEnumerable<string> ConvertToSqlValues();
    }

    public abstract class GridDataProcessorBase : IGridDataProcessor
    {
        public IGridControl GridControl { get; }
        public IColumnTypeConverter TypeConverter { get; }
        public IGridDataFormatter GridDataFormatter { get; }

        protected GridMetadata _metadata;

        public GridDataProcessorBase(IGridControl gridControl, IColumnTypeConverter typeConverter, IGridDataFormatter gridDataFormatter)
        {
            GridControl = gridControl;
            TypeConverter = typeConverter;
            GridDataFormatter = gridDataFormatter;
            _metadata = new GridMetadata(gridControl);
        }

        public abstract DataTable GetSchema();

        public abstract DataTable GridAsDatatable();

        public abstract IEnumerable<string> ConvertToSqlValues();
    }
}