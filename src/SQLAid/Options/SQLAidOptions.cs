using Newtonsoft.Json;
using System;
using System.IO;

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

        public void Save()
        {
            var optionsAsJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(SettingsDirectory, "settings.json"), optionsAsJson);
        }

        public static SQLAidOptions Get()
        {
            if (File.Exists(SettingsFullPath))
                return JsonConvert.DeserializeObject<SQLAidOptions>(File.ReadAllText(SettingsFullPath));

            return new SQLAidOptions();
        }
    }

    public class AlertColor
    {
        public string ServerName { get; set; }
        public string ColorHex { get; set; }
    }
}