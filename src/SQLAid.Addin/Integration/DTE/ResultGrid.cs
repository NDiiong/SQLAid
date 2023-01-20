using Microsoft.SqlServer.Management.UI.Grid;

namespace SQLAid.Integration.DTE
{
    public class ResultGrid : IResultGrid
    {
        private readonly IGridControl _grid;

        public ResultGrid(IGridControl grid)
        {
            _grid = grid;
        }

        public string GetSelectedValue()
        {
            BlockOfCellsCollection cells = _grid.SelectedCells;

            if (cells.Count == 0)
                return null;

            BlockOfCells cellsSet = cells[0];
            IGridStorage storage = _grid.GridStorage;

            return storage.GetCellDataAsString(cellsSet.Y, cellsSet.X);
        }
    }
}