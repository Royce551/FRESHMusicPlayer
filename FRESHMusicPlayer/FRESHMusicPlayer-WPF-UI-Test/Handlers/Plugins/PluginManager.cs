using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Plugins
{
    public class PluginManager
    {
        public List<IPlugin> Plugins { get; private set; } = new List<IPlugin>();

        private MainWindow window;
        private Player player;
        public PluginManager(MainWindow window, Player player)
        {
            this.window = window;
            this.player = player;
            LoadPlugins();
        }
        public void LoadPlugins()
        {
            var assemblies = new[] { typeof(App).GetTypeInfo().Assembly };
            var configuration = new ContainerConfiguration().WithAssembly(typeof(App).GetTypeInfo().Assembly);
            configuration
                .WithAssemblies(
                LoadAssemblies(
                    Directory.GetFiles(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "FRESHMusicPlayer", "Plugins", "FMP-WPF"), "*.dll")));
            using (var container = configuration.CreateContainer())
            {
                foreach (var plugin in container.GetExports<IPlugin>())
                {
                    Plugins.Add(plugin);
                    plugin.Window = window;
                    plugin.Player = player;
                    plugin.Load();
                    Console.WriteLine($"Loaded {plugin.Name} - {plugin.Description}");
                }
            }
        }
        public void UnloadPlugins()
        {
            foreach (var plugin in Plugins) plugin.Unload();
        }
        private IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> paths)
        {
            foreach (var file in paths)
            {
                Assembly assembly;

                assembly = Assembly.LoadFrom(file);

                // Get types now, so that if it throws
                // an exception, it will happen here and get caught,
                // rather than in GetExports where it can't be.
                assembly.GetTypes();

                yield return assembly;
            }
        }
    }
}
