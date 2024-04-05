using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Data;
using Formatting = Newtonsoft.Json.Formatting;

namespace SQLAid.Integration.Files
{
    public class JsonFileService : IFileService, IJsonService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public string AsJson(DataTable datatable)
        {
            return JsonConvert.SerializeObject(datatable, Formatting.Indented, _jsonSerializerSettings);
        }

        public void WriteFile(string path, DataTable datatable)
        {
            var json = JsonConvert.SerializeObject(datatable, Formatting.Indented, _jsonSerializerSettings);

            if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                path += ".json";
            System.IO.File.WriteAllText(path, json);
        }
    }
}