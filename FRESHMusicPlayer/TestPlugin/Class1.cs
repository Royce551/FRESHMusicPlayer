using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Composition;
using FRESHMusicPlayer.Plugins;
using FRESHMusicPlayer;
using System.Windows.Forms;

namespace TestPlugin
{
    [Export(typeof(IPlugin))]
    public class TestPlugin : IPlugin
    {
        public string Name { get; set; } = "External test plugin uwu";
        public string Description { get; set; } = "My external test plugin!";
        public MainWindow Window { get; set; }
        public Player Player { get; set; }

        public void Load()
        {
            Player.SongChanged += Player_SongChanged;
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            MessageBox.Show("hello world!");
        }

        public void Unload()
        {
            Player.SongChanged -= Player_SongChanged;
        }
    }
}
