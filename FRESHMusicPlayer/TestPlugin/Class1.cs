using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Composition;
using FRESHMusicPlayer.Plugins;
using FRESHMusicPlayer;
using System.Windows.Forms;
using System.Media;
using Drawing = System.Drawing;
using System.IO;
using System.Windows.Media;

namespace TestPlugin
{
    [Export(typeof(IPlugin))]
    public class TestPlugin : IPlugin
    {
        public string Name { get; set; } = "Controls Box Colorfier";
        public string Description { get; set; } = "Matches the color of the controls box to the current track.";
        public string Author { get; set; } = "Squid Grill";
        public MainWindow Window { get; set; }
        public Player Player { get; set; }

        private string previousAlbum = string.Empty;

        public void Load()
        {
            Player.SongChanged += Player_SongChanged;
        }

        public void Player_SongChanged(object sender, EventArgs e)
        {
            if (MainWindow.CurrentTrack.EmbeddedPictures.Count() == 0) return;
            if (MainWindow.CurrentTrack.Album == previousAlbum) return; // Assume that all tracks with the same album name will have the same cover art
            using (var bitmap = new Drawing.Bitmap(new MemoryStream(MainWindow.CurrentTrack.EmbeddedPictures[0].PictureData)))
            {
                float red = 0;
                float green = 0;
                float blue = 0;
                int totalpixels = 0;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Drawing.Color pixel = bitmap.GetPixel(x, y);
                        red += pixel.R;
                        green += pixel.G;
                        blue += pixel.B;
                        totalpixels++;
                    }
                }
                if (App.Config.Theme == Skin.Dark)
                {
                    var correctionFactor = 1 + -0.5f;
                    red *= correctionFactor;
                    green *= correctionFactor;
                    blue *= correctionFactor;
                }
                else
                {
                    red = (255 - red) * 0.1f + red;
                    green = (255 - green) * 0.1f + green;
                    blue = (255 - blue) * 0.1f + blue;
                }
                var color = Color.FromRgb((byte)(red / totalpixels), (byte)(green / totalpixels), (byte)(blue / totalpixels));
                    
                    
                Window.ControlsBoxBorderProperty.BorderBrush = Window.ControlsBoxProperty.Background = new SolidColorBrush(color);
            }
            previousAlbum = MainWindow.CurrentTrack.Album;
        }

        public void Unload()
        {
            Player.SongChanged -= Player_SongChanged;
        }
    }
}
