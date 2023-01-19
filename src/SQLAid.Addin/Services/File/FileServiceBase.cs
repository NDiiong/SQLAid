namespace SQLAid.Services.File
{
    public class FileServiceBase
    {
        public static IJsonService JsonService => new JsonFileService();

        public static IFileService GetService(string extension)
        {
            switch (extension)
            {
                case ".xlsx":
                    return new ExcelFileService();

                case ".json":
                    return new JsonFileService();
            }

            return default;
        }
    }
}