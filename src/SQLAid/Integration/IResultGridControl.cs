using Microsoft.SqlServer.Management.UI.Grid;
using System.Collections;

namespace SQLAid.Integration
{
    public interface IResultGridControl
    {
        string GetQueryText();

        object GetGridControl();

        CollectionBase GetGridContainers();

        void ChangeWindowTitle(string text);

        IGridControl GetFocusGridControl();
    }
}