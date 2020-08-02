using ATL.Playlist;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Notifications;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        public ImportPage()
        {
            InitializeComponent();
        }

        private void BrowseTracksButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                MainWindow.Player.AddQueue(dialog.FileName);
                MainWindow.Player.PlayMusic();
            }
        }

        private void BrowsePlaylistsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Playlist Files|*.xspf;*.asx;*.wax;*.wvx;*.b4s;*.m3u;*.m3u8;*.pls;*.smil;*.smi;*.zpl;";
            if (dialog.ShowDialog() == true)
            {
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(dialog.FileName);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                    {
                        MainWindow.NotificationHandler.Add(new NotificationBox(new NotificationInfo("Could not import playlist",
                                                                                                    $"This playlist file could not be imported because one or more of the tracks could not be found.\nMissing File: {s}",
                                                                                                    true,
                                                                                                    true)));
                        continue;
                    }
                    MainWindow.Player.AddQueue(s);
                }
                MainWindow.Player.PlayMusic();
            }
        }

        private void BrowseFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new WinForms.FolderBrowserDialog()) // why do i have to use winforms for this?!
            {
                if (dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(dialog.SelectedPath);           
                    foreach (string s in files)
                    {
                        MainWindow.Player.AddQueue(s);                 
                    }
                    MainWindow.Player.PlayMusic();
                }
            }
        }
    }
}
