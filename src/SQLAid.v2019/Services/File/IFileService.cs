using System.Data;

namespace SQLAid.Services.File
{
    public interface IFileService
    {
        void WriteFile(string path, DataTable datatable);
    }
}