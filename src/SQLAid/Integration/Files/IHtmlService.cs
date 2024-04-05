using System.Data;

namespace SQLAid.Integration.Files
{
    public interface IHtmlService : IFileService
    {
        string ToHtml(DataTable datatable);
    }
}