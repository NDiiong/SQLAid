using ClosedXML.Excel;
using System.Data;

namespace SQLAid.Integration.Files
{
    public class ExcelFileService : IFileService
    {
        public void WriteFile(string path, DataTable datatable)
        {
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var workSheet = workbook.Worksheets.Add(datatable, "GridResult");
                workSheet.Columns().AdjustToContents();
                workbook.SaveAs(path);
            }
        }
    }
}