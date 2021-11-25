using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Utilities;
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

namespace FRESHMusicPlayer.Controls
{
    /// <summary>
    /// Interaction logic for TrackInfo.xaml
    /// </summary>
    public partial class TrackInfo : UserControl
    {
        public TrackInfo()
        {
            InitializeComponent();
        }

        public void Update(IMetadataProvider track)
        {
            InterfaceUtils.SetField(AlbumBox, AlbumLabel, track.Album);
            InterfaceUtils.SetField(GenreBox, GenreLabel, string.Join(", ", track.Genres));
            InterfaceUtils.SetField(YearBox, YearLabel, track.Year.ToString() == "0" ? null : track.Year.ToString());

            InterfaceUtils.SetField(TrackBox, TrackNumberLabel, track.TrackNumber.ToString() == "0" ? null : track.TrackNumber.ToString());
            if (track.TrackTotal > 0) TrackBox.Text += "/" + track.TrackTotal;
            InterfaceUtils.SetField(DiscBox, DiscNumberLabel, track.DiscNumber.ToString() == "0" ? null : track.DiscNumber.ToString());
            if (track.DiscTotal > 0) DiscBox.Text += "/" + track.DiscTotal;

            if (track is FileMetadataProvider file) BitrateBox.Text = file.ATLTrack.Bitrate + "kbps " + (file.ATLTrack.SampleRate / 1000) + "kHz";
            else BitrateBox.Text = "Not available"; // TODO: translate
        }
    }
}
