using Microsoft.SqlServer.Management.UI.Grid;

namespace SQLAid.Integration
{
    public interface IResultGridControl
    {
        IGridControl GetCurrentGridControl();
    }
}