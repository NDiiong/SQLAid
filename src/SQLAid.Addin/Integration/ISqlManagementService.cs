using Microsoft.SqlServer.Management.UI.Grid;
using System;

namespace SQLAid.Integration
{
    public interface ISqlManagementService
    {
        GridControl GetCurrentGridControl(IServiceProvider serviceProvider);

        IGridControl GetCurrentGridControl();
    }
}