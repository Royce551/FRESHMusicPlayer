using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Plugins;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestUIPlugin
{
    [Export(typeof(IPlugin))]
    public class Plugin : IGraphicalPlugin
    {
        public bool HasControlsMenuItem { get; set; } = true;
        public string ControlsMenuItemName { get; set; } = "Open test graphical plugin";
        public string Name { get; set; } = "My test graphical plugin";
        public string Description { get; set; } = "I hope this works!";
        public string Author { get; set; } = "Squid Grill";
        public MainWindow Window { get; set; }
        public Player Player { get; set; }

        public Page GetControlsMenuItem() => new TestControlsMenuItem();

        public void Load()
        {
           
        }

        public void Unload()
        {
            
        }
    }
}
