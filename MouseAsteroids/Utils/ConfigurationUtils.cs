using MouseAsteroids.Models;
using Newtonsoft.Json;
using System.IO;

namespace MouseAsteroids.Utils
{
    public static class ConfigurationUtils
    {
        private static string Path => "./configuration.json";

        public static Configuration Load()
        {
            if (!File.Exists(Path)) Save(new() { Scale = 1 });

            Configuration? configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path));
            if (configuration == null) configuration = new() { Scale = 1 };

            return configuration;
        }

        public static void Save(Configuration configuration)
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(configuration));
        }

    }
}
