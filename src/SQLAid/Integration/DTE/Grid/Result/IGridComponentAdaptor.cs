using SQLAid.Integration.DTE.Grid.Processors;
using System;

namespace SQLAid.Integration.DTE.Grid.Result
{
    public interface IGridComponentAdaptor
    {
        IGridDataProcessor GetGridSelectedProcessor(IGridComponent gridComponent);
    }

    public class GridComponentAdaptor : IGridComponentAdaptor
    {
        public IGridDataProcessor GetGridSelectedProcessor(IGridComponent gridComponent)
        {
            var gridControl = gridComponent.GridControl();
            return new GridSelectedDataProcessor(gridControl, new DefaultColumnTypeConverter(), new DefaultGridDataFormatter());
        }
    }
}