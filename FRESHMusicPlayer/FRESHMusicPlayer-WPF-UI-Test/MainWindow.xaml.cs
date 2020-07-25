using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Winforms = System.Windows.Forms;
namespace FRESHMusicPlayer.Forms.WPF
{
    public enum SelectedMenus
    {
        Tracks,
        Artists,
        Albums,
        Import
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WPFUserInterface : Window
    {
        Winforms.Timer progressTimer;
        public SelectedMenus SelectedMenu = SelectedMenus.Tracks;
        Player player = new Player();
        public WPFUserInterface()
        {
            InitializeComponent();
            player.SongChanged += player_songChanged;
            player.SongStopped += player_songStopped;
            player.SongException += player_songException;
            progressTimer = new Winforms.Timer  // System.Windows.Forms timer because dispatcher timer seems to have some threading issues?
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
        }

        
        #region Controls
        public void PlayPauseMethod()
        {
            if (player.Paused)
            {
                player.ResumeMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            }
            else
            {
                player.PauseMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
            }
        }
        public void StopMethod() => player.StopMusic();
        public void NextTrackMethod() => player.NextSong();
        public void MoreMethod()
        {

        }
        #endregion
        #region Logic
        #region Tabs
        private void ChangeTabs(SelectedMenus tab)
        {
            SelectedMenu = tab;
            UpdateLibrary();
        }
        private void UpdateLibrary()
        {
            TextBlock tab;
            switch (SelectedMenu)
            {
                case SelectedMenus.Tracks:
                    tab = TracksTab;
                    break;
                case SelectedMenus.Artists:
                    tab = ArtistsTab;
                    break;
                case SelectedMenus.Albums:
                    tab = AlbumsTab;
                    break;
                case SelectedMenus.Import:
                    tab = ImportTab;
                    break;
                default:
                    throw new InvalidOperationException("you are idoit");
            }
            TracksTab.FontWeight = ArtistsTab.FontWeight = AlbumsTab.FontWeight = ImportTab.FontWeight = FontWeights.Normal;
            tab.FontWeight = FontWeights.Bold;
        }
        #endregion


        #endregion
        #region Library
        #endregion
        #region Settings
        #endregion
        #region Events
        #region player
        private void player_songStopped(object sender, EventArgs e)
        {
            Title = "FRESHMusicPlayer 8 Development";
            TitleLabel.Text = ArtistLabel.Text = "Nothing Playing";
            progressTimer.Stop();
        }

        private void player_songChanged(object sender, EventArgs e)
        {
            Track track = new Track(player.FilePath);
            Title = $"{track.Artist} - {track.Title} | FRESHMusicPlayer 8 Development";
            TitleLabel.Text = track.Title == "" ? "No Title" : track.Title;
            ArtistLabel.Text = track.Artist == "" ? "No Artist" : track.Artist;
            ProgressBar.Maximum = player.AudioFile.TotalTime.TotalSeconds;
            ProgressIndicator2.Text = player.AudioFile.TotalTime.ToString(@"mm\:ss");

            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                CoverArtArea.Width = new GridLength(5);
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtArea.Width = new GridLength(75);
            }


            progressTimer.Start();
        }
        private void player_songException(object sender, PlaybackExceptionEventArgs e) => MessageBox.Show($"A playback error has occured. \"{e.Details}\"");
        #endregion
        #region ControlsBox
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MoreMethod();
        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PlayPauseMethod();
        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => StopMethod();
        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();
        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e) => player.RepositionMusic((int)ProgressBar.Value);
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (player.Playing)
            {
                player.CurrentVolume = (float)(VolumeBar.Value / 100);
                player.UpdateSettings(); 
            }
        }
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            ProgressIndicator1.Text = player.AudioFile.CurrentTime.ToString(@"mm\:ss");
            ProgressBar.Value = player.AudioFile.CurrentTime.TotalSeconds;
            player.AvoidNextQueue = false;
        }
        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.AudioFile.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
        }
        #endregion
        #region MenuBar
        private void TracksTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Tracks);

        private void ArtistsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Artists);

        private void AlbumsTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Albums);

        private void ImportTab_MouseDown(object sender, MouseButtonEventArgs e) => ChangeTabs(SelectedMenus.Import);
        #endregion
        #endregion




        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //player.AddQueue(FilePathBox.Text);
            player.PlayMusic();                         
        }
       

        private void Window_Closed(object sender, EventArgs e)
        {
            Winforms.Application.Exit();
        }


      

        //private void QueueButton_Click(object sender, RoutedEventArgs e) => player.AddQueue(FilePathBox.Text);

        

        

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                foreach (string path in fileDialog.FileNames) player.AddQueue(path);
                player.PlayMusic();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Q:
                    OpenFileDialog fileDialog = new OpenFileDialog
                    {
                        Multiselect = true
                    };
                    if (fileDialog.ShowDialog() == true)
                    {
                        foreach (string path in fileDialog.FileNames) player.AddQueue(path);
                        player.PlayMusic();
                    }
                    e.Handled = true;
                    break;
            }
        }
    }
}
