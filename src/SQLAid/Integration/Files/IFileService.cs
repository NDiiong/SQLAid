using System.Data;

namespace SQLAid.Integration.Files
{
    public interface IFileService
    {
        void WriteFile(string path, DataTable datatable);
    }
}