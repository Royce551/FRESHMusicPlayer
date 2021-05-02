using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Configuration
{
    public class ConfigurationHandler
    {
        private static string savePath;

        static ConfigurationHandler()
        {
            savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Configuration", "FMP-Avalonia");
        }


        public static async Task<ConfigurationFile> Read()
        {
            if (!File.Exists(Path.Combine(savePath, "config.json"))) await Write(new ConfigurationFile());
            using Stream file = File.OpenWrite(Path.Combine(savePath, "config.json"));

            return await JsonSerializer.DeserializeAsync<ConfigurationFile>(file);
        }

        public static async Task Write(ConfigurationFile config)
        {
            if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            using Stream file = File.OpenWrite(Path.Combine(savePath, "config.json"));

            file.SetLength(0);
            await file.FlushAsync();

            await JsonSerializer.SerializeAsync(file, config);
        }
    }
}
