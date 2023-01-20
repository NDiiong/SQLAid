namespace SQLAid.Integration
{
    public interface IHostContext
    {
        IEditor GetCurrentEditor();

        IEditor GetNewEditor();

        IResultGrid GetFocusedResultGrid();

        IServerConnection CloneCurrentConnection(string database);

        string GetCurrentConnectionString();
    }
}