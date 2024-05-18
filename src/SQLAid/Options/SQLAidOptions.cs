using Newtonsoft.Json;
using System;
using System.IO;

namespace SQLAid.Options
{
    public class SQLAidOptions
    {
        [JsonIgnore] public string LocalData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SqlAidAsyncPackage.NAME);
        [JsonIgnore] public string LogDirectory => Path.Combine(LocalData, "logs");
        [JsonIgnore] public string HistoryDirectory => Path.Combine(LocalData, "histories");
        [JsonIgnore] public string SettingsDirectory => Path.Combine(LocalData, "settings");
        [JsonIgnore] public string TemplateDirectory => Path.Combine(LocalData, "templates");
        [JsonIgnore] public string SettingsFullPath => Path.Combine(SettingsDirectory, "settings.json");

        public AlertColor[] AlertColors { get; set; }

        public SQLAidOptions()
        {
            Directory.CreateDirectory(LocalData);
            Directory.CreateDirectory(LogDirectory);
            Directory.CreateDirectory(HistoryDirectory);
            Directory.CreateDirectory(SettingsDirectory);
            Directory.CreateDirectory(TemplateDirectory);

            AlertColors = Array.Empty<AlertColor>();

            if (File.Exists(SettingsFullPath))
                JsonConvert.PopulateObject(File.ReadAllText(SettingsFullPath), this);
            else
            {
                AlertColors = new AlertColor[] {
                    new AlertColor
                    {
                        ServerName = "localhost",
                        Database = ".",
                        ColorHex = "#ff8080"
                    }
                };

                var optionsAsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFullPath, optionsAsJson);
            }
        }
    }

    public class AlertColor
    {
        public string ServerName { get; set; }
        public string Database { get; set; }
        public string ColorHex { get; set; }
    }
}