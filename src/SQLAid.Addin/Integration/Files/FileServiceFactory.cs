namespace SQLAid.Integration.Files
{
    public static class FileServiceFactory
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