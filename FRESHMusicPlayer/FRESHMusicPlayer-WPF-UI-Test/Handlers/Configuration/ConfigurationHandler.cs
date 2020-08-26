using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Configuration
{
    static class ConfigurationHandler
    {
        public static string ConfigurationPath;
        static ConfigurationHandler()
        {
            ConfigurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                             "FRESHMusicPlayer",
                                             "Configuration",
                                             "FMP-WPF");
        }
        public static ConfigurationFile Read()
        {
            if (!File.Exists(Path.Combine(ConfigurationPath, "config.json")))
            {
                Write(new ConfigurationFile());
            }
            using (StreamReader file = File.OpenText(Path.Combine(ConfigurationPath, "config.json")))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                return (ConfigurationFile)jsonSerializer.Deserialize(file, typeof(ConfigurationFile));
            }
        }
        public static void Write(ConfigurationFile config)
        {
            using (StreamWriter file = File.CreateText(Path.Combine(ConfigurationPath, "config.json")))
            {
                new JsonSerializer().Serialize(file, config);
            }
        }
    }
}
