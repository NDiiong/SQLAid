using Newtonsoft.Json;
using System;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Options
{
    public class SQLAidOptions
    {
        [JsonIgnore] public static string LocalData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SqlAidAsyncPackage.NAME);
        [JsonIgnore] public static string HistoryDirectory => Path.Combine(LocalData, "histories");
        [JsonIgnore] public static string SettingsDirectory => Path.Combine(LocalData, "settings");
        [JsonIgnore] public static string SettingsFullPath => Path.Combine(SettingsDirectory, "settings.json");

        public AlertColor[] AlertColors { get; set; }

        public SQLAidOptions()
        {
            Directory.CreateDirectory(HistoryDirectory);
            Directory.CreateDirectory(SettingsDirectory);
            AlertColors = Array.Empty<AlertColor>();
        }

        public static Task InitOptionsAsync()
        {
            if (File.Exists(SettingsFullPath))
                return Task.CompletedTask;

            var options = new SQLAidOptions
            {
                AlertColors = new AlertColor[] {
                    new AlertColor
                    {
                        ServerName = "ServerName",
                        Database = "DatabaseName",
                        ColorHex = "#ff8080"
                    }
                }
            };

            var optionsAsJson = JsonConvert.SerializeObject(options, Formatting.Indented);
            File.WriteAllText(SettingsFullPath, optionsAsJson);

            return Task.CompletedTask;
        }

        public static SQLAidOptions GetSettings()
        {
            return File.Exists(SettingsFullPath)
                ? JsonConvert.DeserializeObject<SQLAidOptions>(File.ReadAllText(SettingsFullPath))
                : throw new FileNotFoundException("File " + SettingsFullPath + "not found!");
        }
    }

    public class AlertColor
    {
        public string ServerName { get; set; }
        public string Database { get; set; }
        public string ColorHex { get; set; }

        [JsonIgnore] public string ColorKey => $"{ServerName}/{Database}";
    }
}