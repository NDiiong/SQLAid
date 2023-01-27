namespace SQLAid.Integration
{
    public interface IHostContext
    {
        IEditor GetCurrentEditor();

        IEditor GetNewEditor();

        IServerConnection CloneCurrentConnection(string database);

        string GetCurrentConnectionString();
    }
}