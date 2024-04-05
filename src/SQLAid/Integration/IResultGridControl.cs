using Microsoft.SqlServer.Management.UI.Grid;

namespace SQLAid.Integration
{
    public interface IResultGridControl
    {
        string GetQueryText();

        IGridControl GetCurrentGridControl();
    }
}