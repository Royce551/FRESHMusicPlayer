using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Forms
{
    /// <summary>
    /// Interaction logic for PlaylistManagement.xaml
    /// </summary>
    public partial class PlaylistManagement : Window
    {
        private readonly string track;
        public PlaylistManagement(string track)
        {
            InitializeComponent();
            this.track = track;
        }

        private void Okboomer_Click(object sender, RoutedEventArgs e)
        {
            DatabaseUtils.AddTrackToPlaylist(Box.Text, track);
            Close();
        }
    }
}
