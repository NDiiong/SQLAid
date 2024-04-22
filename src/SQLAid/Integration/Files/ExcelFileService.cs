using System.Data;
using System.IO;
using WeihanLi.Npoi;

namespace SQLAid.Integration.Files
{
    public class ExcelFileService : IFileService
    {
        public void WriteFile(string path, DataTable datatable)
        {
            var bytes = datatable.ToExcelBytes();
            File.WriteAllBytes(path, bytes);
        }
    }
}