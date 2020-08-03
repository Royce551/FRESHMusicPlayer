using ATL;
using FRESHMusicPlayer;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FRESHMusicPlayer.Utilities;
namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for QueueManagementPage.xaml
    /// </summary>
    public partial class QueueManagement : Page
    {
        public QueueManagement()
        {
            InitializeComponent();
            PopulateList();
            MainWindow.Player.SongChanged += Player_SongChanged;
        }
        
        public void PopulateList()
        {
            var list = MainWindow.Player.Queue;
            var nextlength = 0;
            int number = 1;
            foreach (var song in list)
            {
                string place;
                if (MainWindow.Player.QueuePosition == number) place = "Playing:";
                else if (MainWindow.Player.QueuePosition == number - 1) place = "Next:";
                else place = (number - MainWindow.Player.QueuePosition).ToString();
                Track track = new Track(song);
                QueueList.Items.Add(new QueueEntry(track.Artist, track.Album, track.Title, place, number - 1));
                if (MainWindow.Player.QueuePosition < number) nextlength += track.Duration;
                number++;
            }
            RemainingTimeLabel.Text = $"Remaining Time - {new TimeSpan(0,0,0,nextlength):mm\\:ss}";
        }
        
        private void Player_SongChanged(object sender, EventArgs e)
        {
            QueueList.Items.Clear();
            PopulateList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.SongChanged -= Player_SongChanged;
            QueueList.Items.Clear();
        }
    }
}
