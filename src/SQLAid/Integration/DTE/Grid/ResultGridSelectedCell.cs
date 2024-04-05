using Microsoft.SqlServer.Management.UI.Grid;

namespace SQLAid.Integration.DTE.Grid
{
    public class ResultGridSelectedCell
    {
        public int ColumnIndex { get; }
        public long RowIndex { get; }

        public ResultGridSelectedCell(long nRowIndex, int nColIndex)
        {
            RowIndex = nRowIndex;
            ColumnIndex = nColIndex;
        }

        public static implicit operator ResultGridSelectedCell(BlockOfCells blockOfCells)
        {
            return new ResultGridSelectedCell(blockOfCells.OriginalY, blockOfCells.OriginalX);
        }
    }
}