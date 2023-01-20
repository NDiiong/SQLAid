using System.Data;

namespace SQLAid.Integration.Files
{
    public interface IJsonService : IFileService
    {
        string AsJson(DataTable datatable);
    }
}