using Microsoft.SqlServer.Management.UI.Grid;
using System.Collections;

namespace SQLAid.Integration
{
    public interface IGridComponent
    {
        string GetQueryText();

        object GetGridControl();

        CollectionBase GetGridContainers();

        void ChangeWindowTitle(string text);

        IGridControl GridControl();
    }

    public interface IResultMessagesControl
    {
        string GetMessages();
    }
}