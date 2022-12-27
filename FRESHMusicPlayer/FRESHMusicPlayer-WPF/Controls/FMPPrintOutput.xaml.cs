using FRESHMusicPlayer.Pages.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FRESHMusicPlayer
{
    /// <summary>
    /// Interaction logic for FMPPrintOutput.xaml
    /// </summary>
    public partial class PrintOutput : UserControl
    {
        public PrintOutput(MainWindow window, string album)
        {
            InitializeComponent();
            CoverArt.Source = BitmapFrame.Create(new MemoryStream(window.CurrentTrack.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.None);
            AlbumName.Text = window.CurrentTrack.Album;
            TracksPanel.Items.Clear();
            int length = 0;
            foreach (var thing in window.Library.ReadTracksForAlbum(album))
            {
                TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, $"{thing.TrackNumber} - {thing.Title}", window, window.NotificationHandler, window.Library));
                length += thing.Length;
            }
            AlbumInfo.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
    }
}
