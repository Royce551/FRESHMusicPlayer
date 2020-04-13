using ATL;
using FRESHMusicPlayer.Handlers;
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
        public WPFUserInterface()
        {
            InitializeComponent();
            Player.songChanged += Player_songChanged;
            Player.songStopped += Player_songStopped;
            Player.songException += Player_songException;
            progressTimer = new Winforms.Timer  // System.Windows.Forms timer because dispatcher timer seems to have some threading issues?
            {
                Interval = 1000
            };
            progressTimer.Tick += ProgressTimer_Tick;
        }

        
        #region Controls
        public void PlayPauseMethod()
        {
            if (Player.paused)
            {
                Player.ResumeMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PauseIcon");
            }
            else
            {
                Player.PauseMusic();
                PlayPauseButton.Data = (Geometry)FindResource("PlayIcon");
            }
        }
        public void StopMethod() => Player.StopMusic();
        public void NextTrackMethod() => Player.NextSong();
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
        #region Player
        private void Player_songStopped(object sender, EventArgs e)
        {
            Title = "FRESHMusicPlayer WPF Test";
            TitleLabel.Text = ArtistLabel.Text = "Nothing Playing";
            progressTimer.Stop();
        }

        private void Player_songChanged(object sender, EventArgs e)
        {
            Track track = new Track(Player.filePath);
            Title = $"{track.Artist} - {track.Title} | FRESHMusicPlayer WPF Test";
            TitleLabel.Text = track.Title;
            ArtistLabel.Text = track.Artist;
            ProgressBar.Maximum = Player.audioFile.TotalTime.TotalSeconds;
            ProgressIndicator2.Text = NumberUtils.Format((int)Player.audioFile.TotalTime.TotalSeconds);

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
        private void Player_songException(object sender, PlaybackExceptionEventArgs e) => MessageBox.Show($"A playback error has occured. \"{e.Details}\"");
        #endregion
        #region ControlsBox
        private void MoreButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MoreMethod();

        private void PlayPauseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => PlayPauseMethod();
        private void StopButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => StopMethod();
        private void NextTrackButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => NextTrackMethod();
        private void ProgressBar_MouseUp(object sender, MouseButtonEventArgs e) => Player.RepositionMusic((int)ProgressBar.Value);
        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.currentvolume = (float)(VolumeBar.Value / 100);
            Player.UpdateSettings();
        }
        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            ProgressIndicator1.Text = NumberUtils.Format((int)Player.audioFile.CurrentTime.TotalSeconds);
            ProgressBar.Value = Player.audioFile.CurrentTime.TotalSeconds;
            Player.avoidnextqueue = false;
        }
        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.audioFile.CurrentTime = TimeSpan.FromSeconds(ProgressBar.Value);
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
            Player.AddQueue(FilePathBox.Text);
            Player.PlayMusic();                         
            Player.currentvolume = .3f;
            Player.UpdateSettings();
        }
       

        private void Window_Closed(object sender, EventArgs e)
        {
            Winforms.Application.Exit();
        }


      

        private void QueueButton_Click(object sender, RoutedEventArgs e) => Player.AddQueue(FilePathBox.Text);

        

        

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = true
            };
            if (fileDialog.ShowDialog() == true)
            {
                foreach (string path in fileDialog.FileNames) Player.AddQueue(path);
                Player.PlayMusic();
            }
        }

        
    }
}
