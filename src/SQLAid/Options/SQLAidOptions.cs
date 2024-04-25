using Newtonsoft.Json;
using System;
using System.IO;

namespace SQLAid.Options
{
    public class SQLAidOptions
    {
        public static string LocalData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SqlAidAsyncPackage.NAME);
        public static string HistoryDirectory => Path.Combine(LocalData, "histories");
        public static string SettingsDirectory => Path.Combine(LocalData, "settings");
        public AlertColor[] AlertColors { get; set; }

        public SQLAidOptions()
        {
            Directory.CreateDirectory(HistoryDirectory);
            Directory.CreateDirectory(SettingsDirectory);
            AlertColors = Array.Empty<AlertColor>();
        }

        public void Save()
        {
            var optionsAsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(SettingsDirectory, "settings.json"), optionsAsJson);
        }

        public static SQLAidOptions Get()
        {
            var content = File.ReadAllText(Path.Combine(SettingsDirectory, "settings.json"));
            return JsonConvert.DeserializeObject<SQLAidOptions>(content);
        }
    }

    public class AlertColor
    {
        public string ServerName { get; set; }
        public string ColorHex { get; set; }
    }
}