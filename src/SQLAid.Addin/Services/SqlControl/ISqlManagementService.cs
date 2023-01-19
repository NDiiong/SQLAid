using Microsoft.SqlServer.Management.UI.Grid;
using System;

namespace SQLAid.Services.SqlControl
{
    public interface ISqlManagementService
    {
        GridControl GetCurrentGridControl(IServiceProvider serviceProvider);
    }
}